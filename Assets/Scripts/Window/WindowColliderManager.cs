using GameState;
using UnityEngine;

namespace Window
{
    [RequireComponent(typeof(SpriteRenderer))]
    [ExecuteAlways]
    public class WindowColliderManager : StateManagedGameObject
    {
        [Header("Collider GameObjects")]
        
        [SerializeField]
        private Transform topCollider;

        [SerializeField]
        private Transform bottomCollider;

        [SerializeField]
        private Transform leftCollider;

        [SerializeField]
        private Transform rightCollider;
        
        [Header("Collider Boundaries")]
        
        [SerializeField]
        public Vector2 innerWallsOffset;

        [SerializeField]
        public Vector2 innerWallsSize;

        [SerializeField]
        private Vector2 outerWallsOffset;

        [SerializeField]
        private Vector2 outerWallsSize;

        // instances
        private SpriteRenderer _renderer;

        // colliders
        protected override void Awake()
        {
            base.Awake();
            
            _renderer = GetComponent<SpriteRenderer>();
            UpdateColliders();
        }

        public override void OnStateChange(State previous, State next)
        {
            // Nothing to do here yet.
        }

        public void UpdateColliders()
        {
            var t = transform;
            var position = t.position;
            var winSize = t.lossyScale;
            var spriteSize = _renderer.size;

            // collider sizes
            topCollider.localScale = new Vector2(
                winSize.x * spriteSize.x + outerWallsSize.x * 2,
                (outerWallsSize.y + outerWallsOffset.y) - (innerWallsSize.y + innerWallsOffset.y));
            bottomCollider.localScale = new Vector2(
                winSize.x * spriteSize.x + outerWallsSize.x * 2,
                (outerWallsSize.y - outerWallsOffset.y) - (innerWallsSize.y - innerWallsOffset.y));
            leftCollider.localScale = new Vector2(
                (outerWallsSize.x - outerWallsOffset.x) - (innerWallsSize.x - innerWallsOffset.x),
                winSize.y * spriteSize.y + outerWallsSize.y * 2);
            rightCollider.localScale = new Vector2(
                (outerWallsSize.x + outerWallsOffset.x) - (innerWallsSize.x + innerWallsOffset.x),
                winSize.y * spriteSize.y + outerWallsSize.y * 2);

            // collider positions
            leftCollider.position = new Vector3(
                position.x + innerWallsOffset.x
                - (
                    (winSize.x * spriteSize.x) / 2
                    + innerWallsSize.x
                    + ((outerWallsSize.x - outerWallsOffset.x) - (innerWallsSize.x - innerWallsOffset.x)) / 2
                ),
                position.y + outerWallsOffset.y);
            rightCollider.position = new Vector3(
                position.x + innerWallsOffset.x
                        + (
                            (winSize.x * spriteSize.x) / 2
                            + innerWallsSize.x
                            + ((outerWallsSize.x + outerWallsOffset.x) - (innerWallsSize.x + innerWallsOffset.x)) / 2
                        ),
                position.y + outerWallsOffset.y);
            topCollider.position = new Vector3(
                position.x + outerWallsOffset.x,
                position.y + innerWallsOffset.y
                        + (
                            (winSize.y * spriteSize.y) / 2
                            + innerWallsSize.y
                            + ((outerWallsSize.y + outerWallsOffset.y) - (innerWallsSize.y + innerWallsOffset.y)) / 2
                        ));
            bottomCollider.position = new Vector3(
                position.x + outerWallsOffset.x,
                position.y + innerWallsOffset.y
                - (
                    (winSize.y * spriteSize.y) / 2
                    + innerWallsSize.y
                    + ((outerWallsSize.y - outerWallsOffset.y) - (innerWallsSize.y - innerWallsOffset.y)) / 2
                ));
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var windowPosition = transform.position;
            // ReSharper disable once Unity.InefficientPropertyAccess
            var windowSize = transform.lossyScale;
            var spriteSize = GetComponent<SpriteRenderer>().size;

            Gizmos.color = Color.magenta;

            // inner
            Gizmos.DrawWireCube(
                windowPosition + (Vector3) innerWallsOffset,
                windowSize * spriteSize + innerWallsSize * 2);
            // outer
            Gizmos.DrawWireCube(
                windowPosition + (Vector3) outerWallsOffset,
                windowSize * spriteSize + outerWallsSize * 2);
        }
#endif
    }
}