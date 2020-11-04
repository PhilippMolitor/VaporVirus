using System;
using System.Collections;
using GameState;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Antivirus
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(AudioSource))]
    public class AntivirusController : StateManagedGameObject
    {
        [SerializeField]
        private new Camera camera;

        [SerializeField] 
        private float sweepDuration;

        [SerializeField]
        private float sweepDelayMin;
        
        [SerializeField]
        private float sweepDelayMax;
        
        // state
        private SpriteRenderer _renderer;
        private AudioSource _audioSource;
        private BoxCollider2D _collider;
        private bool _scannerEnabled;
        private bool _scannerSweeping = false;
        private Vector2 _lockPosition;
        private float _scannerPosition = 0f;

        protected override void Awake()
        {
            base.Awake();
            
            _renderer = GetComponent<SpriteRenderer>();
            _audioSource = GetComponent<AudioSource>();
            _collider = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            UpdateScannerScale();
            UpdateScannerPosition();
        }
        
        public override void OnStateChange(State previous, State next)
        {
            switch (next)
            {
                case State.INGAME:
                    _collider.enabled = false;
                    StartCoroutine(RunFirstStart());
                    break;
                case State.GAMEOVER_UI:
                case State.MENU_UI:
                case State.CREATED:
                case State.DYING:
                    SetSweepingActive(false, true);
                    StopAllCoroutines();
                    _audioSource.Stop();
                    _renderer.enabled = false;
                    break;
            }
        }

        private IEnumerator RunFirstStart()
        { 
            yield return new WaitForSeconds(sweepDelayMax);
            SetSweepingActive(true);
        }

        private void SetSweepingActive(bool status, bool force = false)
        {
            _scannerEnabled = status;
            
            if(status)
                StartCoroutine(RunSweeping());
            else if (force)
                StopCoroutine(RunSingleScan());
        }
        
        private void UpdateScannerScale()
        {
            var distanceY = camera.orthographicSize * 2f * (1f / transform.localScale.y);
            _renderer.size = new Vector2(_renderer.size.x, distanceY);
        }
        
        private void UpdateScannerPosition()
        {
            var distanceX = camera.orthographicSize * camera.aspect;
            var rendererSize = _renderer.size;
            var localScale = transform.localScale;
            var startPositionX = _lockPosition.x - distanceX + (rendererSize.x / 2) * localScale.x;
            var endPositionX = _lockPosition.x + distanceX - (rendererSize.x / 2) * localScale.x;
            
            // ReSharper disable once Unity.InefficientPropertyAccess
            transform.position = new Vector3(
                startPositionX + (endPositionX - startPositionX) * _scannerPosition,
                camera.transform.position.y);
        }
        
        private IEnumerator RunSweeping()
        {
            while (_scannerEnabled)
            {
                yield return RunSingleScan();
                yield return new WaitForSeconds(Random.Range(sweepDelayMin, sweepDelayMax));
            }
        } 

        private IEnumerator RunSingleScan()
        {
            _renderer.enabled = true;
            _scannerSweeping = true;
            _collider.enabled = true;
            _lockPosition = camera.transform.position;
            _audioSource.Play();
            
            var reversed = _scannerPosition >= 0.5f;
            var startTime = Time.time;
            
            var progress = 0f;
            while (progress < 1f)
            {
                progress = Math.Min((startTime - Time.time) / sweepDuration * -1f, 1f);
                _scannerPosition = reversed ? 1f - progress : progress;
                yield return new WaitForEndOfFrame();
            }
            
            _audioSource.Stop();
            _scannerSweeping = false;
            _renderer.enabled = false;
            _collider.enabled = false;
        }
    }
}
