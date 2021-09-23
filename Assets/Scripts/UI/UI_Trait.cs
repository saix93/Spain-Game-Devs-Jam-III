using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Trait : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI Text;

    public void Init(SO_Trait trait)
    {
        Text.text = $"{trait.Name} (+{trait.Value})";
    }
}
