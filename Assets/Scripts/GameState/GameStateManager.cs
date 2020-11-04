using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameState
{
    public enum State
    {
        CREATED,
        MENU_UI,
        INGAME,
        DYING,
        GAMEOVER_UI
    }
    
    public class GameStateManager
    {
        // singleton instance
        private static GameStateManager _instance;
        
        // state
        private State _state = State.MENU_UI;
        private readonly List<StateManagedGameObject> _managedObjects = new List<StateManagedGameObject>();
        
        // singleton constructor
        public static GameStateManager Instance => _instance ?? (_instance = new GameStateManager());

        public void AddManagedGameObject(StateManagedGameObject obj)
        {
            _managedObjects.Add(obj);
        }

        public void RemoveManagedGameObject(StateManagedGameObject obj)
        {
            _managedObjects.Remove(obj);
        }

        public State GetState() => _state;
        
        public void SetState(State next)
        {
            var previous = _state;
            _state = next;

            if (previous == next)
            {
#if UNITY_EDITOR
                Debug.Log("Not changing state: already at state " + next);
#endif
                return;
            }
            
#if UNITY_EDITOR
            Debug.Log("Exiting GameState: " + previous);
#endif
            
            foreach (var managedObject in _managedObjects.ToArray())
            {
                try
                {
                    managedObject.OnStateChange(previous, next);
                }
                catch
                {
                    Debug.LogWarning("Failed to update a managedObject's GameState: " + managedObject.name);
                }
            }
            
#if UNITY_EDITOR
            Debug.Log("Entered new GameState: " + next);
#endif
        }
    }
}