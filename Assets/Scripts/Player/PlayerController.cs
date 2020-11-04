using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using GameState;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput = Input.PlayerInput;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PolygonCollider2D))]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : StateManagedGameObject, PlayerInput.IPlayerActions
    {
        [Header("Ground Movement")]
    
        [SerializeField] 
        [Range(0, 10)]
        private float groundMovementSpeed;

        [SerializeField]
        [Range(1, 10)]
        private float movementSmoothing;
    
        [SerializeField] 
        private AnimationCurve groundLocomotionCurve;
    
        [Header("Airborne Movement")]
    
        [SerializeField] 
        private float airborneMovementSpeed;

        [SerializeField]
        private float airborneMaxDistance;

        [SerializeField]
        private AnimationCurve airborneAccelerationCurve;

        [SerializeField]
        [Range(0, 1)]
        private float jumpProgressMinDuration;

        [SerializeField]
        [Range(1, 10)]
        private float jumpProgressFillDuration;

        [SerializeField] 
        private float airborneCooldownTime;

        [SerializeField] 
        private JumpChargeRenderer chargeProgressRenderer;

        [Header("Camera Behavior")] 
        
        [SerializeField]
        private CinemachineVirtualCamera airborneCamera;

        [Header("Animators")] 
        
        [SerializeField] 
        private RuntimeAnimatorController movingAnimatorController;
        
        [SerializeField] 
        private RuntimeAnimatorController jumpAnimatorController;
        
        [SerializeField] 
        private RuntimeAnimatorController deathAnimatorController;
        
        [Header("Audio")] 
        
        [SerializeField] 
        private AudioClip moveSound;

        [SerializeField] 
        private AudioClip landingSound;
        
        [SerializeField] 
        private AudioClip jumpSound;
        
        [SerializeField] 
        private AudioClip dyingSound;
        
        // instances
        private Rigidbody2D _rigidbody;
        private Animator _animator;
        private AudioSource _audioSource;
        private PlayerInput _input;

        // player state
        private Vector2 _inputDirection;
        private Vector2 _moveDirection;
        private Vector2 _lookDirection = new Vector2(1f, 0f);
        private bool _isInsideWindow;
        private bool _isAirborne;
        private bool _jumpChargeActive;
        private float _jumpRequestedTime;
        private float _jumpStrength;
        private float _lastAirborneDoneTime;

        public override void OnStateChange(State previous, State next)
        {
            switch (next)
            {
                case State.INGAME:
                    _input.Player.Enable();
                    break;
                case State.DYING:
                    _input.Player.Disable();
                    StopAllCoroutines();
                    StartCoroutine(WaitToGameOver());
                    break;
                case State.GAMEOVER_UI:
                    _audioSource.Stop();
                    break;
            }
        }

        // DEBUG, remove later!
        private IEnumerator WaitToGameOver()
        {
            _animator.runtimeAnimatorController = deathAnimatorController;
            _audioSource.clip = dyingSound;
            _audioSource.loop = false;
            _audioSource.Play();
            
            yield return new WaitForSecondsRealtime(2);
            StateManager.SetState(State.GAMEOVER_UI);
        }

        private void OnDisable()
        {
            _input?.Player.Disable();
        }

        // GameObject is created
        protected override void Awake()
        {
            // create input handler
            _input = new PlayerInput();
            _input.Player.SetCallbacks(this);
            
            base.Awake();
            
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
        }

        // Graphical + Input updates
        private void Update()
        {
            if(GameStateManager.Instance.GetState() == State.INGAME) 
                UpdatePlayerSpriteState();
        }

        // Physics + Movement updates
        private void FixedUpdate()
        {
            if (GameStateManager.Instance.GetState() == State.INGAME) 
                UpdatePlayerPosition();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Player fucked up the jump
            if(other.gameObject.layer == LayerMask.NameToLayer("Ground"))
                _isInsideWindow = true;
            // Antivirus cought the Player
            else if (other.gameObject.layer == LayerMask.NameToLayer("Antivirus"))
                HandleAntivirusHit();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
                _isInsideWindow = false;
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            _inputDirection = ctx.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext ctx)
        {
            if (ctx.started
                && !_isAirborne
                && _lastAirborneDoneTime + airborneCooldownTime < Time.time)
            {
                // initiate charge
                _jumpChargeActive = true;
                _jumpRequestedTime = Time.unscaledTime;
                
                _moveDirection = Vector2.zero;
                _inputDirection = Vector2.zero;
                
                StartCoroutine(UpdateJumpStrength(_jumpRequestedTime));
                StartCoroutine(UpdateJumpProgressIndicator());
            } 
            else if (ctx.canceled && _jumpChargeActive)
            {
                // end charge and take action
                _jumpChargeActive = false;
                
                if (_jumpRequestedTime + jumpProgressMinDuration < Time.unscaledTime)
                    StartCoroutine(ExecuteJump());
            }
        }

        public void OnAbortJump(InputAction.CallbackContext context)
        {
            if(_jumpChargeActive)
                _jumpChargeActive = false;
        }

        private void UpdatePlayerPosition()
        {
            if (_isAirborne || _jumpChargeActive) return;

            // update look direction
            if (_moveDirection.magnitude >= 0.1f) _lookDirection = _moveDirection.normalized;

            // lerp between actual motion direction and desired motion direction
            _moveDirection = Vector2.Lerp(
                _moveDirection, 
                Vector2.ClampMagnitude(_inputDirection, 1),
                Time.deltaTime * movementSmoothing);
        
            // move player to lerped position
            _rigidbody.MovePosition(
                _rigidbody.position 
                + (groundMovementSpeed * groundLocomotionCurve.Evaluate(Time.time) * Time.fixedDeltaTime) 
                * _moveDirection);
            
            // play sound & animation if not playing yet
            if (_audioSource.clip != moveSound || !_audioSource.isPlaying)
            {
                if (moveSound)
                {
                    _audioSource.clip = moveSound;
                    _audioSource.loop = true;
                    _audioSource.Play();
                }

                _animator.runtimeAnimatorController = movingAnimatorController;
            } else if (_audioSource.clip == moveSound && _moveDirection.magnitude < 0.1f)
            {
                _audioSource.Stop();
                _animator.runtimeAnimatorController = null;
            }
            
#if UNITY_EDITOR
            // Show debugging: move direction and desired input direction
            var debugPos = _rigidbody.position;
            Debug.DrawRay(debugPos, _moveDirection * 4f, Color.red);
            Debug.DrawRay(debugPos, _inputDirection * 4f, Color.blue);
#endif
        }

        private void UpdatePlayerSpriteState()
        {
            transform.rotation = Quaternion.LookRotation(Vector3.back, _lookDirection)
                                 * Quaternion.Euler(0, 0, 90);
        } 

        private IEnumerator UpdateJumpProgressIndicator()
        {
            while (_jumpChargeActive)
            {
                chargeProgressRenderer.SetProgress(_jumpStrength);
                yield return new WaitForEndOfFrame();
            }
            
            chargeProgressRenderer.SetProgress(0f);
        }

        private IEnumerator UpdateJumpStrength(float startTime)
        {
            while (_jumpChargeActive)
            {
#if UNITY_EDITOR
                // show current destination
                Debug.DrawRay(
                    _rigidbody.position,
                    _lookDirection * (airborneMaxDistance * _jumpStrength),
                    Color.magenta);
#endif
            
                _jumpStrength = Math.Min((Time.unscaledTime - startTime) / jumpProgressFillDuration, 1f);
                
                yield return null;
            }
            
#if UNITY_EDITOR
            // show flight path
            Debug.DrawRay(
                _rigidbody.position,
                _lookDirection * (airborneMaxDistance * _jumpStrength),
                Color.green,
                2f);
#endif
        }
        
        private IEnumerator ExecuteJump()
        {
            // lift off
            _isAirborne = true;
            gameObject.layer = LayerMask.NameToLayer("PlayerAirborne");
        
            // switch camera
            if(airborneCamera) airborneCamera.enabled = true;
            
            var startPos = _rigidbody.position;
            var endPos = startPos + (_lookDirection * (airborneMaxDistance * _jumpStrength));
            var progress = 0f;

            // sound
            _audioSource.clip = jumpSound;
            _audioSource.loop = false;
            _audioSource.Play();
            
            // animation
            _animator.runtimeAnimatorController = jumpAnimatorController;
            
            while(progress < 1f)
            {
                var currentPos = _rigidbody.position;
                progress = (startPos - currentPos).magnitude / (endPos - startPos).magnitude;
                
                _rigidbody.MovePosition(
                    currentPos
                    + (_lookDirection * (
                           airborneMovementSpeed 
                           * Time.fixedDeltaTime 
                           * airborneAccelerationCurve.Evaluate(Math.Min(progress, 1f))
                           )));
                
                yield return new WaitForFixedUpdate();
            }

            // reset values
            _jumpStrength = 0f;
            
            // landing
            gameObject.layer = LayerMask.NameToLayer("Player");
            _isAirborne = false;
            _lastAirborneDoneTime = Time.time;
            
            // animator
            _animator.runtimeAnimatorController = null;
        
            // switch camera
            if(airborneCamera) airborneCamera.enabled = false;
        
            HandleJumpResult();
        }

        private void HandleJumpResult()
        {
            if (_isInsideWindow)
            {
                // successful jump
                _audioSource.clip = landingSound;
                _audioSource.loop = false;
                _audioSource.Play();
            }
            else
            {
                // failed jump, game over
                StateManager.SetState(State.DYING);
            }
        }

        private void HandleAntivirusHit()
        {
            Debug.Log("Hit by Antivirus!");
            StateManager.SetState(State.DYING);
        }  
    }
}
