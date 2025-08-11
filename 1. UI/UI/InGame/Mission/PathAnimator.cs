using System;
using System.Collections.Generic;
using System.Threading;
using _1.Scripts.UI.InGame.HUD;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace _1.Scripts.UI.InGame.Mission
{
    public class PathAnimator : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] Transform player; 
        [SerializeField] GameObject markerPrefab; 

        [Header("설정")] 
        [SerializeField] float updateInterval = 5f;
        [SerializeField] float markerSpeed = 30f;

        private Queue<GameObject> markers = new();
        private Transform target;
        private NavMeshPath navPath;
        private CancellationTokenSource cts;

        private void Awake()
        {
            navPath = new NavMeshPath();
            if (!player)
            {
                var go = GameObject.FindWithTag("Player");
                if (go) player = go.transform;
            }
        }

        private void OnEnable()
        {                       
            DistanceUI.OnTargetChanged += OnTargetChanged;
            cts = new CancellationTokenSource();
            if (DistanceUI.CurrentTarget)
                OnTargetChanged(DistanceUI.CurrentTarget);
            PathUpdateLoop(cts.Token).Forget();
        }

        private void OnDisable()
        {
            DistanceUI.OnTargetChanged -= OnTargetChanged;
            
            while (markers.TryDequeue(out var marker)) Destroy(marker);
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }

        private void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }

        private void OnTargetChanged(Transform newTarget)
        {
            target = newTarget;
            if (!player || !player.gameObject.activeInHierarchy)
            {
                var go = GameObject.FindWithTag("Player");
                if (go) player = go.transform;
            }
            if (!player || !target) return;
            var corners = GetPathCorners(player.position, target.position);
            if (corners.Length > 1)
                AnimateMarkerAlongPath(corners, cts?.Token ?? CancellationToken.None).Forget();
        }
        
        private async UniTaskVoid PathUpdateLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!player || !target)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(updateInterval), cancellationToken: token);
                    continue;
                }

                var corners = GetPathCorners(player.position, target.position);
                if (corners.Length > 1) AnimateMarkerAlongPath(corners, token).Forget();

                await UniTask.Delay(TimeSpan.FromSeconds(updateInterval), cancellationToken: token, cancelImmediately: true);
            }
        }

        private async UniTask AnimateMarkerAlongPath(Vector3[] corners, CancellationToken token)
        {
            if (corners.Length < 2 || !player) return;

            corners[0] = player.position;
            var marker = Instantiate(markerPrefab, corners[0], Quaternion.identity); 
            SetupMarkerVisual(marker);
            
            markers.Enqueue(marker);
            for (int i = 1; i < corners.Length; i++)
            {
                while (marker && Vector3.Distance(marker.transform.position, corners[i]) > 0.05f)
                {
                    marker.transform.position = Vector3.MoveTowards(
                        marker.transform.position,
                        corners[i],
                        markerSpeed * Time.deltaTime
                    );
                    await UniTask.Yield(token, cancelImmediately: true);
                }
            }

            if (!markers.TryDequeue(out var deqMarker)) return;
            Destroy(deqMarker, 0.5f);
        }
        
        private void SetupMarkerVisual(GameObject marker)
        {
            var lt = marker.GetComponentInChildren<Light>();
            lt.type = LightType.Point;
            lt.range = 1f;
            lt.intensity = 1f;
            lt.color = Color.cyan;

            var trail = marker.GetComponentInChildren<TrailRenderer>();
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.time = 1.0f;
            trail.startWidth = 0.1f;
            trail.endWidth = 0.0f;

            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.cyan, 0f), new GradientColorKey(Color.cyan, 1f) },
                new[] { new GradientAlphaKey(0.2f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            trail.colorGradient = grad;
        }
        
        private Vector3[] GetPathCorners(Vector3 startPos, Vector3 targetPos)
        {
            navPath.ClearCorners();
            NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, navPath);
            return navPath.corners;
        }
    }
}