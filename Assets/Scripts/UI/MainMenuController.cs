using System;
using System.Collections;
using GameState;
using Score;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class MainMenuController : StateManagedGameObject
    {
        [SerializeField]
        private GameObject gameStartPanel;
        
        [SerializeField]
        private GameObject controlSchemePanel;
        
        [SerializeField]
        private Canvas mainMenuCanvas;

        [SerializeField] 
        private Canvas gameOverCanvas;

        [SerializeField] 
        private TextMeshProUGUI scoreValueText;

        [SerializeField]
        private TextMeshProUGUI mailCountText;

        private SpriteRenderer _renderer;
        private int _unreadLeft = 2;

        protected override void Awake()
        {
            base.Awake();
            _renderer = GetComponent<SpriteRenderer>();
            mailCountText.text = "test";
        }

        protected void Update()
        {
            mailCountText.text = "2 Items, " + _unreadLeft + " Unread";
        }

        public override void OnStateChange(State previous, State next)
        {
            switch (next)
            {
                case State.MENU_UI:
                    gameOverCanvas.enabled = false;
                    mainMenuCanvas.enabled = true;
                    StartCoroutine(ShowWindow(true));
                    break;
                case State.INGAME:
                    StartCoroutine(ShowWindow(false));
                    mainMenuCanvas.enabled = false;
                    gameOverCanvas.enabled = false;
                    break;
                case State.GAMEOVER_UI:
                    mainMenuCanvas.enabled = false;
                    gameOverCanvas.enabled = true;
                    scoreValueText.text = ScoreManager.Instance.GetScore().ToString();
                    StartCoroutine(ShowWindow(true));
                    break;
            }
        }

        public void ShowGamePanel()
        {
            if (_unreadLeft > 0) _unreadLeft--;
            
            gameStartPanel.SetActive(true);
            controlSchemePanel.SetActive(false);
        }

        public void ShowControlPanel()
        {
            if (_unreadLeft > 0) _unreadLeft--;
            
            controlSchemePanel.SetActive(true);
            gameStartPanel.SetActive(false);
        }

        public void StartGameRound()
        {
            StateManager.SetState(State.INGAME);
        }

        private IEnumerator ShowWindow(bool state)
        {
            var start = Time.time;
            var progress = 0f;

            if (state) _renderer.enabled = true;
            
            // animation
            while (progress < 1f)
            {
                progress = (Time.time - start) / 1f;
                if (state)
                    transform.localScale = Vector2.one * Mathf.Clamp01(progress);
                else
                    transform.localScale = Vector2.one * Mathf.Clamp01(1f - progress);

                yield return null;
            }
            
            // disable canvas
            _renderer.enabled = state;
        }
    }
}
