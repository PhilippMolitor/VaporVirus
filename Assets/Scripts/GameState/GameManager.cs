using Background;
using Cinemachine;
using Score;
using UnityEngine;
using UnityEngine.InputSystem;
using Window;

namespace GameState
{
    public class GameManager : StateManagedGameObject
    {
        [Header("Cameras")]
    
        [SerializeField] 
        private CinemachineVirtualCamera menuCamera;
    
        [SerializeField] 
        private CinemachineVirtualCamera playerCamera;

        [Header("Spawn")]
    
        [SerializeField] 
        private Transform playerSpawnPoint;

        [SerializeField] 
        private GameObject playerPrefab;

        [SerializeField] 
        private GameObject windowPrefab;

        [SerializeField] 
        private Vector2 spawnWindowSize;

        [Header("Misc")] 
    
        [SerializeField]
        private BackgroundOffsetController backgroundOffsetController;

        private GameObject _activePlayer;
    
        public override void OnStateChange(State previous, State next)
        {
            switch (next)
            {
                case State.INGAME:
                    ScoreManager.Instance.ClearScore();
                    SetupSpawn();
                    menuCamera.gameObject.SetActive(false);
                    Cursor.visible = false;
                    break;
                case State.MENU_UI:
                case State.GAMEOVER_UI:
                    menuCamera.gameObject.SetActive(true);
                    if(_activePlayer) Destroy(_activePlayer);
                    Cursor.visible = true;
                    break;
                default:
                    Cursor.visible = true;
                    break;
            }
        }

        private void SetupSpawn()
        {
            playerCamera.gameObject.SetActive(false);
            
            var window = Instantiate(windowPrefab);
            window.transform.position = playerSpawnPoint.position;
            window.GetComponent<WindowManager>().Resize(spawnWindowSize);
            
            var player = Instantiate(playerPrefab);
            player.transform.position = playerSpawnPoint.position;
        
            _activePlayer = player;
            playerCamera.m_Follow = player.transform;
            backgroundOffsetController.followedObject = player.transform;
            
            playerCamera.gameObject.SetActive(true);
        }
    }
}
