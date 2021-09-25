using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chair : MonoBehaviour
{
    [Header("Data")]
    public int ID;
    public List<Chair> LinkedChairs;
    public Vector2 CharacterSitPositionOffset;
    public Character AssignedCharacter;
    public float GizmoRadius = .3f;

    public Vector3 GetCharacterPosition()
    {
        return transform.position + (Vector3)CharacterSitPositionOffset;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        var tPos = transform.position;
        Gizmos.DrawWireSphere(tPos, GizmoRadius);

        foreach (var chair in LinkedChairs)
        {
            var chPos = chair.transform.position;
            
            Gizmos.DrawLine(tPos, chPos);
        }
    }
}
