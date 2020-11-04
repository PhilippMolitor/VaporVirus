using System;
using GameState;
using Score;
using TMPro;
using UnityEngine;

namespace UI
{
    public class ScoreValueController : StateManagedGameObject
    {
        [SerializeField] 
        private Canvas scoreUiCanvas;

        [SerializeField]
        private TextMeshProUGUI scoreValueText;

        private ScoreManager _scoreManager;

        protected override void Awake()
        {
            base.Awake();
            _scoreManager = ScoreManager.Instance;
        }

        public override void OnStateChange(State previous, State next)
        {
            switch (next)
            {
                case State.INGAME:
                    scoreUiCanvas.enabled = true;
                    break;
                default:
                    scoreUiCanvas.enabled = false;
                    break;
            }
        }

        private void Update()
        {
            scoreValueText.text = _scoreManager.GetScore().ToString();
        }
    }
}
