using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Image))]
    public class ImageCanvasAnimator : MonoBehaviour
    {
        [SerializeField]
        private RuntimeAnimatorController controller;
    
        private Image _imageCanvas;
        private SpriteRenderer _fakeRenderer;
        private Animator _animator;
        
        protected void Start ()
        {
            _imageCanvas = GetComponent<Image>();
            _fakeRenderer = gameObject.AddComponent<SpriteRenderer>();
            _animator = gameObject.AddComponent<Animator>();

            _fakeRenderer.enabled = true;
            _animator.runtimeAnimatorController = controller;
        }
    
        protected void Update () {
            if (_animator.runtimeAnimatorController) {
                _imageCanvas.sprite = _fakeRenderer.sprite;
            }
        }
    
    }
}