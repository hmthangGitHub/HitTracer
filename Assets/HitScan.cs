using System;
using SimpleMan.VisualRaycast;
using UnityEngine;

public class HitScan : MonoBehaviour
{
    [SerializeField] private Transform _transform;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var castResult = this.Raycast(castAll: true, originPoint: _transform.position, _transform.forward);
        }
    }
}