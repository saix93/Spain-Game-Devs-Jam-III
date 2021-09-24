using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_MainCanvas : MonoBehaviour
{
    [Header("References")]
    public List<TextMeshProUGUI> States;

    private void Update()
    {
        var currentState = GameManager._.GetCurrentState();
        
        // TODO: States en blanco y en rojo el actual
        States.ForEach(s => s.color = Color.white);

        States[(int)currentState].color = Color.red;
    }
}
