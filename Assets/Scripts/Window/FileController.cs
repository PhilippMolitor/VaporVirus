using System;
using System.Collections;
using System.Collections.Generic;
using GameState;
using Score;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Window
{
    public class FileController : StateManagedGameObject
    {
        [SerializeField] 
        private Material glitchMaterial;
       
        [SerializeField]
        private AudioSource _audioSource;
        
        [SerializeField]
        private SpriteRenderer _renderer;

        [SerializeField] 
        private TextMeshProUGUI fileNameText;

        [SerializeField] 
        private List<string> nameParts;

        [SerializeField] 
        private List<string> extensions;

        [SerializeField]
        private List<AudioClip> progressSounds;

        [SerializeField] 
        private AudioClip destroySound;
        
        public override void OnStateChange(State previous, State next)
        {
            
        }

        protected override void Awake()
        {
            base.Awake();
            var part1 = nameParts[Random.Range(0, nameParts.Count)];
            var part2 = nameParts[Random.Range(0, nameParts.Count)];
            var ext = extensions[Random.Range(0, extensions.Count)];

            fileNameText.text = part1 + "_" + part2 + "." + ext;
            Debug.Log(fileNameText.text);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            StartCoroutine(SelfDestroy());
        }

        private IEnumerator SelfDestroy()
        {
            _renderer.material = glitchMaterial;
            _audioSource.clip = progressSounds[Random.Range(0, progressSounds.Count)];
            _audioSource.Play();
            yield return new WaitForSeconds(0.6f);
            
            // destroy
            ScoreManager.Instance.IncrementDestroyedFileCount();
            _audioSource.clip = destroySound;
            _audioSource.Play();
            Destroy(gameObject);
        }
    }
}
