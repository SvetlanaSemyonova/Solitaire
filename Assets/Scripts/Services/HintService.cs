using Solitaire.Models;
using System.Collections.Generic;

namespace Solitaire.Services
{
    public class HintService : IHintService
    {
        readonly Game _game;

        public HintService(Game game)
        {
            _game = game;
        }

        public Pile FindValidMove(Card card)
        {
            if (card == null)
            {
                return null;
            }

            var pileTarget = CheckPilesForMove(_game.PileFoundations, card);

            if (pileTarget == null)
            {
                pileTarget = CheckPilesForMove(_game.PileTableaus, card);
            }

            return pileTarget;
        }

        public Pile CheckPilesForMove(IList<Pile> piles, Card card)
        {
            for (var i = 0; i < piles.Count; i++)
            {
                var pile = piles[i];

                if (pile.CanAddCard(card))
                {
                    return pile;
                }
            }

            return null;
        }

        public Hint GetHint()
        {
            Hint hint = null;

            for (var i = 0; i < _game.PileTableaus.Count; i++)
            {
                var pileTableau = _game.PileTableaus[i];

                for (var j = 0; j < pileTableau.Cards.Count; j++)
                {
                    if (TryGenerateMoveHint(pileTableau.Cards[j], out hint))
                    {
                        return hint;
                    }
                }
            }

            if (_game.PileWaste.HasCards && TryGenerateMoveHint(_game.PileWaste.TopCard(), out hint))
            {
                return hint;
            }

            if (_game.PileStock.HasCards && _game.PileStock.TopCard().IsDrawable)
            {
                hint = new Hint()
                {
                    Card = _game.PileStock.TopCard(),
                    Pile = _game.PileWaste,
                };
            }

            return hint;
        }

        private bool TryGenerateMoveHint(Card card, out Hint hint)
        {
            hint = null;

            if (card == null || !card.IsMoveable)
            {
                return false;
            }

            var pile = FindValidMove(card);

            if (pile == null)
            {
                return false;
            }

            hint = new Hint()
            {
                Card = card,
                Pile = pile,
            };

            return true;
        }
    }
}
