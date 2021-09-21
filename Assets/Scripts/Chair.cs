using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chair : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer Visual;

    [Header("Data")]
    public List<Chair> LinkedChairs;
    public Vector2 CharacterSitPositionOffset;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        var tPos = transform.position;

        foreach (var chair in LinkedChairs)
        {
            var chPos = chair.transform.position;
            
            Gizmos.DrawWireSphere(tPos, .3f);
            Gizmos.DrawLine(tPos, chPos);
            Gizmos.DrawWireSphere(chPos, .3f);
        }
    }
}
