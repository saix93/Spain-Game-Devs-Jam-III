using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Data")]
    public Vector2 CharacterSitPositionOffset;
    public float GizmoRadius = .5f;

    [Header("RealtimeData")]
    public Character AssignedCharacter;

    public Vector3 GetCharacterPosition()
    {
        return transform.position + (Vector3)CharacterSitPositionOffset;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        var tPos = transform.position;
        Gizmos.DrawWireSphere(tPos, GizmoRadius);
    }
}
