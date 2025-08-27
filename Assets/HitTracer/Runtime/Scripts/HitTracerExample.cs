using System;
using System.Linq;
using HitTrace;
using UnityEngine;

public class HitTracerExample : MonoBehaviour
{
    [SerializeField] HitTracer hitTracer;
    
    public async void Attack()
    {
        try
        {
            Debug.Log("Hit Trace");
            var hitTraceResult = await hitTracer.HitTrace(false);
            Debug.Log($"Hit Trace {string.Join(',', hitTraceResult.Select(x => x.Key.name).OrderBy(x => x))}");
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }
}
