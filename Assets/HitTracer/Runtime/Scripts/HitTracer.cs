using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SimpleMan.VisualRaycast;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Vector3 = UnityEngine.Vector3;

namespace HitTrace
{
    public partial class HitTracer : MonoBehaviour
    {
        public enum HitTraceType
        {
            Ray,
            Sphere,
        }
    
        [SerializeField] private int numberOfCheckPoints;
        [SerializeField] private int historyCount;
        [SerializeField] private HitTraceType hitTraceType = HitTraceType.Ray;
        [SerializeField] private float sphereRadius;
        [HideInInspector] [SerializeField] private Transform hitTracePointContainer;
        [HideInInspector] [SerializeField] private List<Vector3> hitTraceLocalPoints;
    
        private RingBuffer<List<Vector3>> histories;
        private readonly Dictionary<Collider, (RaycastHit raycastHit, Ray ray)> hitTraceResult = new();
        private UniTaskCompletionSource<IReadOnlyDictionary<Collider, (RaycastHit raycastHit, Ray ray)>> hitTraceResultUcs;

        private void LateUpdate()
        {
            CalculateHitTrace();
            UpdateHistories();
        }

        private void UpdateHistories()
        {
            histories ??= new RingBuffer<List<Vector3>>(historyCount);
            histories.Resize(historyCount);
        
            var currentHistory = histories.GetBackItem();
            currentHistory.Clear();
            foreach (var t in hitTraceLocalPoints)
            {
                currentHistory.Add(this.transform.TransformPoint(t));
            }
            histories.Add(currentHistory);
        }

        private void CalculateHitTrace()
        {
            if (hitTraceResultUcs == null) return;
            hitTraceResultUcs.TrySetResult(HitTraceInternal());
            hitTraceResultUcs = null;
        }

        public UniTask<IReadOnlyDictionary<Collider, (RaycastHit raycastHit, Ray ray)>> HitTrace(bool immediate = true)
        {
            if (immediate)
            {
                return UniTask.FromResult(HitTraceInternal());
            }
            else
            {
                hitTraceResultUcs?.TrySetCanceled();
                hitTraceResultUcs = new();
                return hitTraceResultUcs.Task;
            }
        }

        private IReadOnlyDictionary<Collider, (RaycastHit raycastHit, Ray ray)> HitTraceInternal()
        {
            hitTraceResult.Clear();
            foreach (var history in histories.IterateLatestToEarliest())
            {
                for (int hitPointIndex = 0; hitPointIndex < hitTraceLocalPoints.Count; hitPointIndex++)
                {
                    var direction = this.transform.TransformPoint(hitTraceLocalPoints[hitPointIndex]) - history[hitPointIndex];

                    var result = Cast(history, hitPointIndex, direction);

                    foreach (var rayCastHit in result.Hits)
                    {
                        if (!hitTraceResult.TryGetValue(rayCastHit.collider, out var rayCastResult) 
                            || rayCastResult.raycastHit.distance > rayCastHit.distance)
                        {
                            hitTraceResult[rayCastHit.collider] = (rayCastHit, new Ray(history[hitPointIndex], direction));
                        }
                    }
                }
            }
            return hitTraceResult;
        }

        private CastResult Cast(List<Vector3> history, int hitPointIndex, Vector3 direction)
        {
            CastResult result;
            if (hitTraceType == HitTraceType.Ray)
            {
                result = this.Raycast(castAll: true, history[hitPointIndex], direction, direction.magnitude, ignoreSelf: true);
            }
            else
            {
                result = this.SphereCast(castAll: true, history[hitPointIndex], direction, sphereRadius, direction.magnitude, ignoreSelf: true);
            }

            return result;
        }
    }
}