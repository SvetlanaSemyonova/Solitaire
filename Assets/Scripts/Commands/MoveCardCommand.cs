using Solitaire.Models;
using Solitaire.Services;
using System;
using Zenject;

namespace Solitaire.Commands
{
    public class MoveCardCommand : ICommand, IDisposable, IPoolable<Card, Pile, Pile, IMemoryPool>
    {
        [Inject] readonly IAudioService _audioService;
        [Inject] readonly IPointsService _pointsService;
        [Inject] readonly Game.Config _gameConfig;

        Card _card;
        Pile _pileSource;
        Pile _pileTarget;
        IMemoryPool _pool;

        private bool _wasTopCardFlipped;

        public void Execute()
        {
            if (_pileSource.TopCard() == _card)
            {
                _pileTarget.AddCard(_card);
            }
            else
            {
                var cards = _pileSource.SplitAt(_card);
                _pileSource.RemoveCards(cards);
                _pileTarget.AddCards(cards);
            }

            if (_pileSource.IsWaste)
            {
                if (_pileTarget.IsTableau)
                {
                    _pointsService.Add(_gameConfig.PointsWasteToTableau);
                }
                else if (_pileTarget.IsFoundation)
                {
                    _pointsService.Add(_gameConfig.PointsWasteToFoundation);
                }
            }
            else if (_pileSource.IsTableau && _pileTarget.IsFoundation)
            {
                _pointsService.Add(_gameConfig.PointsTableauToFoundation);
            }
            else if (_pileSource.IsFoundation && _pileTarget.IsTableau)
            {
                _pointsService.Add(_gameConfig.PointsFoundationToTableau);
            }

            _audioService.PlaySfx(Audio.SfxDraw, 0.5f);

            var cardBelow = _pileSource.TopCard();

            if (_pileSource.IsTableau &&
                cardBelow != null && !cardBelow.IsFaceUp.Value)
            {
                cardBelow.Flip();
                _wasTopCardFlipped = true;
                _pointsService.Add(_gameConfig.PointsTurnOverTableauCard);
            }
        }

        public void Undo()
        {
            var cardTop = _pileSource.TopCard();

            if (_pileSource.IsTableau && _wasTopCardFlipped &&
                cardTop != null && cardTop.IsFaceUp.Value)
            {
                cardTop.Flip();
                _pointsService.Add(-_gameConfig.PointsTurnOverTableauCard);
            }

            if (_pileSource.IsWaste)
            {
                if (_pileTarget.IsTableau)
                {
                    _pointsService.Add(-_gameConfig.PointsWasteToTableau);
                }
                else if (_pileTarget.IsFoundation)
                {
                    _pointsService.Add(-_gameConfig.PointsWasteToFoundation);
                }
            }
            else if (_pileSource.IsTableau && _pileTarget.IsFoundation)
            {
                _pointsService.Add(-_gameConfig.PointsTableauToFoundation);
            }
            else if (_pileSource.IsFoundation && _pileTarget.IsTableau)
            {
                _pointsService.Add(-_gameConfig.PointsFoundationToTableau);
            }

            _audioService.PlaySfx(Audio.SfxDraw, 0.5f);

            if (_pileTarget.TopCard() == _card)
            {
                _pileSource.AddCard(_card);
            }
            else
            {
                var cards = _pileTarget.SplitAt(_card);
                _pileTarget.RemoveCards(cards);
                _pileSource.AddCards(cards);
            }
        }

        public void Dispose()
        {
            _pool.Despawn(this);
        }

        public void OnDespawned()
        {
            _card = null;
            _pileSource = null;
            _pileTarget = null;
            _pool = null;
        }

        public void OnSpawned(Card card, Pile pileSource, Pile pileTarget, IMemoryPool pool)
        {
            _card = card;
            _pileSource = pileSource;
            _pileTarget = pileTarget;
            _pool = pool;
            _wasTopCardFlipped = false;
        }

        public class Factory : PlaceholderFactory<Card, Pile, Pile, MoveCardCommand>
        {
        }
    }
}
