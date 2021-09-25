using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Data")]
    public float GizmoRadius = .5f;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        var tPos = transform.position;
        Gizmos.DrawWireSphere(tPos, GizmoRadius);
    }
}
