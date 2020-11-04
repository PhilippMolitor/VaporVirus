using System;
using UnityEngine;

namespace GameState
{
    public abstract class StateManagedGameObject : MonoBehaviour
    {
        protected GameStateManager StateManager = GameStateManager.Instance;

        protected virtual void Awake()
        {
            StateManager.AddManagedGameObject(this);
        }

        protected virtual void Start()
        {
            OnStateChange(State.CREATED, StateManager.GetState());
        }

        protected virtual void OnDestroy()
        {
            StateManager.RemoveManagedGameObject(this);
        }

        public abstract void OnStateChange(State previous, State next);
    }
}