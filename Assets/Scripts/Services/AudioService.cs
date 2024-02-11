using DG.Tweening;
using Solitaire.Models;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Services
{
    public class AudioService : IAudioService
    {
        readonly Dictionary<string, AudioClip> _audioMap;
        readonly Audio.Config _audioConfig;
        Transform _camTransform;
        AudioSource _music;
        Tweener _tweenFadeIn;
        Tweener _tweenFadeOut;

        float _volumeSfx = 1f;
        float _volumeMusic = 1f;

        const float FadeDuration = 2.0f;

        public AudioService(Audio.Config audioConfig)
        {
            _audioConfig = audioConfig;
            _audioMap = new Dictionary<string, AudioClip>(_audioConfig.AudioClips.Count);

            for (var i = 0; i < _audioConfig.AudioClips.Count; i++)
            {
                var clip = _audioConfig.AudioClips[i];
                _audioMap.Add(clip.name, clip);
            }
        }

        public void SetVolume(float volume)
        {
            _volumeSfx = volume;
            _volumeMusic = volume;
        }

        public void PlaySfx(string key, float volume)
        {
            if (!_audioMap.TryGetValue(key, out var clip))
            {
                Debug.LogWarning($"Couldn't find '{key}' AudioClip.");
                return;
            }

            if (_camTransform == null)
            {
                _camTransform = Camera.main.transform;
            }

            var vol = _volumeSfx * volume;

            if (vol <= 0f)
            {
                return;
            }

            AudioSource.PlayClipAtPoint(clip, _camTransform.position, vol);
        }

        public void PlayMusic(string key, float volume)
        {
            if (!_audioMap.TryGetValue(key, out var clip))
            {
                Debug.LogWarning($"Couldn't find '{key}' AudioClip.");
                return;
            }

            var vol = _volumeMusic * volume;

            if (vol <= 0f)
            {
                return;
            }

            if (_music == null)
            {
                var go = new GameObject("Music");
                _music = go.AddComponent<AudioSource>();
                _music.spatialBlend = 0;
                _music.volume = 0;
                _music.loop = true;
                _music.volume = 0f;
                _music.clip = clip;
                _music.Play();
            }
            else
            {
                _music.clip = clip;
            }

            _tweenFadeOut?.Pause();

            if (_tweenFadeIn == null)
            {
                _tweenFadeIn = _music.DOFade(vol, FadeDuration)
                    .SetEase(Ease.InQuad)
                    .SetAutoKill(false)
                    .OnRewind(() => _music.Play());
            }
            else
            {
                _tweenFadeIn.Restart();
            }
        }

        public void StopMusic()
        {
            if (_music == null)
            {
                return;
            }

            _tweenFadeIn?.Pause();

            if (_tweenFadeOut == null)
            {
                _tweenFadeOut = _music.DOFade(0f, FadeDuration)
                    .SetEase(Ease.OutQuad)
                    .SetAutoKill(false)
                    .OnComplete(() => _music.Stop());
            }
            else
            {
                _tweenFadeOut.Restart();
            }
        }
    }
}
