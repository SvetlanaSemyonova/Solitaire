using Solitaire.Helpers;
using Solitaire.Services;
using UniRx;

namespace Solitaire.Models
{
    public class Options : DisposableEntity
    {
        public BoolReactiveProperty DrawThree { get; private set; } = new BoolReactiveProperty(false);
        public BoolReactiveProperty AudioEnabled { get; private set; } = new BoolReactiveProperty(true);
        public BoolReactiveProperty RestartNeeded { get; private set; } = new BoolReactiveProperty(false);
        public ReactiveCommand CloseCommand { get; private set; }

        readonly Game _game;
        readonly GameState _gameState;
        readonly GamePopup _gamePopup;

        public Options(Game game, GameState gameState, GamePopup gamePopup, IAudioService audioService)
        {
            _game = game;
            _gameState = gameState;
            _gamePopup = gamePopup;

            CloseCommand = new ReactiveCommand(gamePopup.State.Select(s => s == Game.Popup.Options));
            CloseCommand.Subscribe(_ => Close()).AddTo(this);

            DrawThree.Where(_ => _game.HasStarted.Value).Subscribe(_ => RestartNeeded.Value = true).AddTo(this);

            AudioEnabled.Subscribe(audioEnabled =>
            {
                audioService.SetVolume(audioEnabled ? 1f : 0f);

                if (audioEnabled)
                    audioService.PlayMusic(Audio.Music, 0.3333f);
                else
                    audioService.StopMusic();
            }).AddTo(this);
        }

        private void Close()
        {
            if (RestartNeeded.Value)
            {
                if (_gameState.State.Value == Game.State.Paused)
                {
                    _game.RestartCommand.Execute();
                }
                else
                {
                    _game.HasStarted.Value = false;
                    _gamePopup.State.Value = Game.Popup.None;
                }

                RestartNeeded.Value = false;
            }
            else
            {
                if (_gameState.State.Value == Game.State.Paused)
                {
                    _game.ContinueCommand.Execute();
                }
                else
                {
                    _gamePopup.State.Value = Game.Popup.None;
                }
            }
        }
    }
}
