using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_MainCanvas : MonoBehaviour
{
    [Header("References")]
    public GameObject ButtonComenceFeast;

    private void Update()
    {
        ButtonComenceFeast.SetActive(GameManager._.GetCurrentState() == UnionStates.PreparingFeast);
    }
}
