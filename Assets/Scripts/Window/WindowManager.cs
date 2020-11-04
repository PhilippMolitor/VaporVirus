using System;
using System.Collections;
using System.Collections.Generic;
using GameState;
using Level;
using Score;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using State = GameState.State;

namespace Window
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(WindowColliderManager))]
    public class WindowManager : StateManagedGameObject
    {
        [Header("Window Animations")] [SerializeField] [Range(0, 2)]
        private float openCloseAnimationDuration;

        [Header("File spawning")] [SerializeField] [Range(0, 20)]
        private int minFiles = 3;

        [SerializeField] [Range(0, 20)] private int maxFiles = 10;

        [SerializeField] private List<GameObject> filePrefabs;

        [SerializeField] private Vector2 gridSize;

        [Header("UI")] [SerializeField] private RectTransform uiCanvas;

        [SerializeField] private TextMeshProUGUI titleText;

        [SerializeField] private TextMeshProUGUI fileCountText;

        // state
        private bool _hasBeenVisited;

        // instances
        private readonly ScoreManager _scoreManager = ScoreManager.Instance;
        private SpriteRenderer _renderer;
        private WindowColliderManager _windowColliderManager;
        private LevelGenerator _levelGenerator;

        public void Resize(Vector2 size)
        {
            _renderer.size = size;
            _windowColliderManager.UpdateColliders();
        }

        public IEnumerator AnimateWindow(bool open = true)
        {
            var start = Time.time;
            var progress = 0f;

            // animation
            while (progress < 1f)
            {
                progress = (Time.time - start) / openCloseAnimationDuration;
                if (open)
                    transform.localScale = Vector2.one * Mathf.Clamp01(progress);
                else
                    transform.localScale = Vector2.one * Mathf.Clamp01(1f - progress);

                yield return null;
            }

            if (open)
            {
                // update the wall colliders!
                _windowColliderManager.UpdateColliders();
                StartCoroutine(SpawnFiles());
            }
            else
                // remove window
                Destroy(gameObject);
        }

        public override void OnStateChange(State previous, State next)
        {
            switch (next)
            {
                case State.DYING:
                    StartCoroutine(AnimateWindow(false));
                    break;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _levelGenerator = FindObjectOfType<LevelGenerator>();
            _renderer = GetComponent<SpriteRenderer>();
            _windowColliderManager = GetComponent<WindowColliderManager>();
            //titleText.text = "Hello World!";
            //fileCountText.text = "99 Bottles of Beer";
        }

        private void Update()
        {
            uiCanvas.sizeDelta = _renderer.size;
            fileCountText.text = "Score: " + _scoreManager.GetScore();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_hasBeenVisited && other.gameObject.CompareTag("Player"))
            {
                HandleFirstVisit();
                _hasBeenVisited = true;
            }
        }

        private void HandleFirstVisit()
        {
            _scoreManager.IncrementVisitedWindowCount();

            // generate next windows
            StartCoroutine(_levelGenerator.RequestWindowGeneration(transform.position));
        }

        private List<Vector2> FindFilePositions()
        {
            var start = new Vector2(
                transform.position.x
                + _windowColliderManager.innerWallsOffset.x
                - ((transform.lossyScale.x * _renderer.size.x) / 2)
                - (_windowColliderManager.innerWallsSize.x - _windowColliderManager.innerWallsOffset.x) * 2,
                transform.position.y
                + _windowColliderManager.innerWallsOffset.y
                + ((transform.lossyScale.y * _renderer.size.y) / 2)
                + (_windowColliderManager.innerWallsSize.y - _windowColliderManager.innerWallsOffset.y) * 2
            );

            var end = new Vector2(
                transform.position.x
                + _windowColliderManager.innerWallsOffset.x
                + ((transform.lossyScale.x * _renderer.size.x) / 2)
                + (_windowColliderManager.innerWallsSize.x - _windowColliderManager.innerWallsOffset.x) * 2,
                transform.position.y
                + _windowColliderManager.innerWallsOffset.y
                - ((transform.lossyScale.y * _renderer.size.y) / 2)
                - (_windowColliderManager.innerWallsSize.y - _windowColliderManager.innerWallsOffset.y) * 2
            );

            var countX = Math.Abs(Math.Floor((end.x - start.x) / gridSize.x));
            var countY = Math.Abs(Math.Floor((end.y - start.y) / gridSize.y));

            Debug.Log(countX + " x " + countY);

#if UNITY_EDITOR
            Debug.DrawLine(start, end, Color.magenta, 10f);
#endif

            var positions = new List<Vector2>();

            for (var y = 2; y < countY; y++)
            {
                for (var x = 2; x < countX; x++)
                {
                    var mid = new Vector2(
                        start.x + (x * gridSize.x) - (gridSize.x / 2),
                        start.y - (y * gridSize.y) + (gridSize.y / 2));
                    Debug.DrawRay(mid, new Vector2(0.5f, -0.5f), Color.black, 10f);
                    positions.Add(mid);
                }
            }

            return positions;
        }

        private IEnumerator SpawnFiles()
        {
            var positions = FindFilePositions();
            var fileCount = Random.Range(minFiles, Math.Min(positions.Count, maxFiles));
            var current = 0;

            foreach (var pos in positions)
            {
                if (current >= fileCount) break;

                var file = Instantiate(filePrefabs[Random.Range(0, filePrefabs.Count)], pos, Quaternion.identity,
                    transform);
                current += 1;
                yield return null;
            }
        }
    }
}