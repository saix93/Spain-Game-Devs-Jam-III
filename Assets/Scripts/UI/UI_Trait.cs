using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Trait : MonoBehaviour
{
    [Header("References")]
    public Image Icon;

    public void Init(SO_Trait trait)
    {
        Icon.sprite = trait.Icon;
    }
}
