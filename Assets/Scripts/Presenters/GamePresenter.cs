using Solitaire.Models;
using Solitaire.Services;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Solitaire.Presenters
{
    public class GamePresenter : MonoBehaviour
    {
        [SerializeField] Physics2DRaycaster _cardRaycaster;
        [SerializeField] PilePresenter _pileStock;
        [SerializeField] PilePresenter _pileWaste;
        [SerializeField] PilePresenter[] _pileDecks;
        [SerializeField] PilePresenter[] _pileSlots;
    
        [Inject] readonly Game _game;
        [Inject] readonly GameState _gameState;
        [Inject] readonly OrientationState _orientation;
        [Inject] readonly IAudioService _audioService;

        Camera _camera;
        int _layerInteractable;

        const float CamSizeLandscape = 4.25f;
        const float CamSizePortrait = 8.25f;

        private void Awake()
        {
            _camera = Camera.main;
            _layerInteractable = LayerMask.NameToLayer("Interactable");
        }

        private void Start()
        {
            _orientation.State.Subscribe(AdjustCamera).AddTo(this);
            _gameState.State.Pairwise().Subscribe(HandleGameStateChanges).AddTo(this);
            _game.Init(_pileStock.Pile, _pileWaste.Pile, 
                _pileDecks.Select(p => p.Pile).ToList(), 
                _pileSlots.Select(p => p.Pile).ToList());

            SetCameraLayers(true);
        }

        private void Update()
        {
            if (_gameState.State.Value == Game.State.Playing)
            {
                _game.DetectWinCondition();
            }
        }

        private void AdjustCamera(Orientation orientation)
        {
            _camera.orthographicSize = (orientation == Orientation.Landscape ? 
                CamSizeLandscape : CamSizePortrait);
        }

        private void HandleGameStateChanges(Pair<Game.State> state)
        {
            if (state.Previous == Game.State.Home)
            {
                SetCameraLayers(false);
                _audioService.PlayMusic(Audio.Music, 0.3333f);
            }
            else if (state.Current == Game.State.Home)
            {
                SetCameraLayers(true);
                _audioService.StopMusic();
            }

            _cardRaycaster.enabled = state.Current == Game.State.Playing;
        }

        private void SetCameraLayers(bool cullGame)
        {
            if (cullGame)
            {
                _camera.cullingMask = ~(1 << _layerInteractable);
            }
            else
            {
                _camera.cullingMask = ~0;
            }
        }
    }
}
