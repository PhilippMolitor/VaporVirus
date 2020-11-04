using System;
using System.Collections;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class JumpChargeRenderer : MonoBehaviour
    {
        [SerializeField]
        private Sprite[] progressFrames;

        [SerializeField]
        private Sprite[] fullLoopFrames;

        [SerializeField] 
        [Range(0, 1)]
        private float fullLoopFrameTime;

        private SpriteRenderer _renderer;
        private float _progress = 0f;
        private int _currentLoopFrame = 0;
        private bool _fullLoopActive = false;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _progress = 0f;
        }

        private void Update()
        {
            // hide renderer if progress is 0
            if (Math.Abs(_progress) < 0.01f)
            {
                if(_renderer.enabled) _renderer.enabled = false;
                return;
            }

            // enable renderer if needed
            if (!_renderer.enabled)
            {
                _currentLoopFrame = 0;
                _renderer.enabled = true;
            }
            
            if (_progress < 1f)
            {
                // show corresponding frame
                var frame = Mathf.FloorToInt(
                    RemapValue(_progress, 0f, 1f, 0f, progressFrames.Length));
                _renderer.sprite = progressFrames[frame];
            }
            else if(_progress >= 1f && !_fullLoopActive)
            {
                // loop through "full" animation frames
                _fullLoopActive = true;
                StartCoroutine(AdvanceFullLoopFrames());
            }
        }

        private IEnumerator AdvanceFullLoopFrames()
        {
            while (_progress >= 1f)
            {
                if (_currentLoopFrame == fullLoopFrames.Length) _currentLoopFrame = 0;
                _renderer.sprite = fullLoopFrames[_currentLoopFrame];
                _currentLoopFrame += 1;
                
                yield return new WaitForSecondsRealtime(fullLoopFrameTime);
            }

            _fullLoopActive = false;
        }

        public void SetProgress(float progress)
        {
            if (progress < 0f)
                _progress = 0f;
            else if(progress > 1f)
                _progress = 1f;
            else
                _progress = progress;
        }
        
        private static float RemapValue( float value, float leftMin, float leftMax, float rightMin, float rightMax )
        {
            return rightMin + (value - leftMin) * (rightMax - rightMin) / (leftMax - leftMin);
        }

    }
}
