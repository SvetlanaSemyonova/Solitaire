using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Solitaire.Helpers
{
    public abstract class StateReactor<T> : MonoBehaviour where T : Enum
    {
        [Tooltip("List of states in which this object should be visible.")]
        [SerializeField]
        List<T> _visibleInStates;

        protected abstract StateModel<T> Model { get; }

        private void Start()
        {
            Model.State.Subscribe(state => SetVisibility(IsVisible(state))).AddTo(this);
        }

        protected abstract void SetVisibility(bool isVisible);

        private bool IsVisible(T state)
        {
            if (state == null)
            {
                return false;
            }

            return _visibleInStates.Contains(state);
        }
    }
}
