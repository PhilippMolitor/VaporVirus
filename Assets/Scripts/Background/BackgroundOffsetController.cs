using GameState;
using UnityEngine;

namespace Background
{
    public class BackgroundOffsetController : StateManagedGameObject
    {
        public Transform followedObject;

        [SerializeField] 
        private Transform mainMenu;
        
        [SerializeField] 
        private Vector2 offsetMultiplier;
    
        private SpriteRenderer _renderer;
        private static readonly int PositionOffset = Shader.PropertyToID("PositionOffset");

        protected override void Awake()
        {
            base.Awake();
            _renderer = GetComponent<SpriteRenderer>();
        }

        public override void OnStateChange(State previous, State next)
        {
            switch (next)
            {
                case State.MENU_UI:
                case State.GAMEOVER_UI:
                    followedObject = mainMenu;
                    break;
            }
        }

        private void Update()
        {
            if (!followedObject) return;
            
            var targetPos = followedObject.position;
            transform.position = targetPos;
            _renderer.material.SetVector(PositionOffset, targetPos * offsetMultiplier);
        }
    }
}
