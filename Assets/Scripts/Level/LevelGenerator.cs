using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameState;
using UnityEngine;
using Window;
using Random = UnityEngine.Random;

namespace Level
{ 
    public class LevelGenerator : StateManagedGameObject
    {
        [Header("Spawning Settings")]
        
        [SerializeField]
        [Range(1, 10)]
        private int minNewWindowCount;

        [Range(1, 10)]
        [SerializeField]
        private int maxNewWindowCount;
        
        [SerializeField]
        [Range(10, 30)]
        private float minSpawnDistance;

        [SerializeField]
        [Range(10, 30)]
        private float maxSpawnDistance;

        [SerializeField]
        [Range(0, 20)]
        private float minSpawnReverseDistance;
        
        [SerializeField]
        [Range(0, 10)]
        private float timeBetweenSpawns;

        [Header("Window Settings")]
        
        [SerializeField]
        private GameObject windowPrefab;
        
        [SerializeField]
        [Range(4, 24)]
        private float windowMinWidth;
        
        [SerializeField]
        [Range(4, 24)]
        private float windowMaxWidth;

        [SerializeField]
        [Range(4, 24)]
        private float windowMinHeight;
        
        [SerializeField]
        [Range(4, 24)]
        private float windowMaxHeight;

        [Header("RayCast Finder Precision")]
        
        [SerializeField]
        private int radialRayCount;

        public override void OnStateChange(State previous, State next)
        {
            // Nothing will happen here directly
        }

        public IEnumerator RequestWindowGeneration(Vector2 origin)
        {
            var windowCount = Random.Range(minNewWindowCount, maxNewWindowCount);

            for (var i = 0; i < windowCount; i++)
            {
                var distance = Random.Range(minSpawnDistance, maxSpawnDistance);
                var size = new Vector2(
                    Random.Range(windowMinWidth, windowMaxWidth),
                    Random.Range(windowMinHeight, windowMaxHeight));
                
                var locations = GetPossibleWindowLocations(origin, size, distance);

                // are there any possibilities?
                if (!locations.Any()) continue;
                
                var location = locations.ElementAt(Random.Range(0, locations.Count()));
                SpawnWindowAt(location, size);
                
                yield return new WaitForSeconds(timeBetweenSpawns);
            }
        }

        public void SpawnWindowAt(Vector2 position, Vector2 size)
        {
                var window = Instantiate(windowPrefab, position, Quaternion.identity);
                window.transform.localScale = Vector2.zero;
                window.SetActive(true);
                
                var windowManager = window.GetComponent<WindowManager>();
                windowManager.Resize(size);
                StartCoroutine(windowManager.AnimateWindow());
        }

        private List<Vector2> GetPossibleWindowLocations(Vector3 origin, Vector2 dimensions, float distance)
        {
            var possibleLocations = new List<Vector2>();

            for (var i = 0; i < 360; i += 360 / radialRayCount)
            {
                var targetCenter = Quaternion.Euler(0, 0, i) * Vector2.up * distance;
                var center = origin + targetCenter;
                
                var checkCorners = new[]
                {
                    new Vector2(-dimensions.x / 2, +dimensions.y / 2), // top left 
                    new Vector2(+dimensions.x / 2, +dimensions.y / 2), // top right 
                    new Vector2(-dimensions.x / 2, -dimensions.y / 2), // bottom left
                    new Vector2(+dimensions.x / 2, -dimensions.y / 2) // bottom right
                };

                var hasObstacle = false;
                
                // check if window fits in this place
                foreach (var corner in checkCorners)
                {
                    var hit = Physics2D.Raycast(
                        center, 
                        corner.normalized, 
                        corner.magnitude,
                        LayerMask.GetMask("Wall"));
                    
#if UNITY_EDITOR
                    Debug.DrawRay(
                        center, 
                        corner, 
                        hit.collider ? Color.red : Color.green, 
                        5);
#endif
                    
                    // next ray check if this one already collided
                    if (!hit.collider) continue;
                    
                    hasObstacle = true;
                    break;
                }

                // no need to check further
                if (hasObstacle) continue;

                // check min distance to other existing windows
                for (var j = 0; j < 360; j += 360 / radialRayCount)
                {
                    var end = Quaternion.Euler(0, 0, j) * Vector2.up * minSpawnReverseDistance;
                    var hit = Physics2D.Raycast(
                        center, 
                        end.normalized, 
                        end.magnitude,
                        LayerMask.GetMask("Wall"));
                    
                    // is too close to another existing window
                    if (!hit.collider) continue;
                    
                    hasObstacle = true;
                    break;
                }
                
                // if nothing was hit, this is a nice location for our window
                if(!hasObstacle) possibleLocations.Add(center);
            }

            return possibleLocations;
        }
    }
}
