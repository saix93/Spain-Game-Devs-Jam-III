using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Character : MonoBehaviour
{
    [Header("References")]
    public UI_Trait UITraitPrefab;
    public Transform TraitContainer;
    public TextMeshProUGUI SadnessText;

    private Character currentCharacter;

    private void Update()
    {
        var sadness = Utils.CalculateSadness(currentCharacter);
        SadnessText.text = $"{sadness.SadnessLevel.ToString()} ({currentCharacter.SadnessPoints})";
    }

    public void Init(Character character)
    {
        currentCharacter = character;
        
        Utils.DestroyChildren(TraitContainer);
        currentCharacter.Traits.ForEach(t =>
        {
            var i = Instantiate(UITraitPrefab, TraitContainer);
            i.Init(t);
        });
    }
}
