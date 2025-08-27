#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SimpleMan.VisualRaycast;
using UnityEditor;
using UnityEngine;

namespace HitTrace
{
    public partial class HitTracer
    {
        [SerializeField] private float paddingTop;
        [SerializeField] private float paddingBottom;
    
        [ContextMenu("SetUp")]
        private void SetUp()
        {
            EditorApplication.delayCall -= SetUp;
        
            try
            {
                hitTraceLocalPoints.Clear();
                if (transform == null) return;
            }
            catch (Exception e)
            {
                return;
            }

            var meshRenderers = this.GetComponentsInChildren<MeshRenderer>();
            var meshColliders = new List<MeshCollider>();
            foreach (var meshRenderer in meshRenderers)
            {
                if (!meshRenderer.TryGetComponent<MeshCollider>(out var meshCollider))
                {
                    meshCollider = meshRenderer.gameObject.AddComponent<MeshCollider>();
                    meshColliders.Add(meshCollider);
                }
            }

            var bounds = TryGetCombinedMeshColliderBounds(this.gameObject);
            var z = bounds.max.z;
            var yMin = bounds.min.y;
            var yMax = bounds.max.y;

            var minPoint = new Vector3(0, yMin + paddingBottom, z + 0.5f) + this.transform.position;
            var maxPoint = new Vector3(0, yMax - paddingTop, z + 0.5f) + this.transform.position;

            for (int i = 0; i < numberOfCheckPoints; i++)
            {
                var point = Vector3.Lerp(minPoint, maxPoint, i / ((float)numberOfCheckPoints - 1));
                CreateHitPointFromPoint(point);
            }

            foreach (var meshCollider in meshColliders)
            {
                DestroyImmediate(meshCollider);
            }

            EditorUtility.SetDirty(this);
        }
    
        private static void DebugSphere(int index, Transform t, Color color)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = $"Debug {index}";
            sphere.transform.position = t.position;
            sphere.transform.localScale = Vector3.one * 0.1f;
            Destroy(sphere.GetComponent<Collider>());
            sphere.GetComponent<Renderer>().material.color = color;
            UniTask.Void(async () =>
            {
                await UniTask.DelayFrame(5);
                Destroy(sphere);
            });
        }

        private void CreateHitPointFromPoint(Vector3 point)
        {
            var result = this.Raycast(point, -Vector3.forward, ignoreSelf: false);
            hitTraceLocalPoints.Add(this.transform.InverseTransformPoint(result.FirstHit.point));
        }
    
    
        private static Bounds TryGetCombinedMeshColliderBounds(GameObject root)
        {
            var meshColliders = root.GetComponentsInChildren<Collider>(true);
            var combined = new Bounds();
            for (int i = 0; i < meshColliders.Length; i++)
            {
                combined.Encapsulate(meshColliders[i].bounds);
            }

            return combined;
        }

        private void OnDrawGizmosSelected()
        {
            foreach (var hitTracePoint in hitTraceLocalPoints)
            {
                if (hitTraceType == HitTraceType.Sphere)
                {
                    Gizmos.DrawWireSphere(transform.TransformPoint(hitTracePoint), sphereRadius);
                }
                else
                {
                    Gizmos.DrawSphere(transform.TransformPoint(hitTracePoint), 0.01f);
                }
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
#endif
            EditorApplication.delayCall += SetUp;
        }
    }
}

#endif