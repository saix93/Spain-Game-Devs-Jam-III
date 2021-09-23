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
    public TextMeshProUGUI IsPriestText;

    private Character character;

    private void Update()
    {
        var sadness = Utils.CalculateSadness(character);
        SadnessText.text = $"{sadness.SadnessLevel.ToString()} ({character.SadnessPoints})";
        IsPriestText.text = $"IsPriest: {character.IsPriest}";
    }

    public void Init(Character newCharacter)
    {
        character = newCharacter;
        
        Utils.DestroyChildren(TraitContainer);
        character.Traits.ForEach(t =>
        {
            var i = Instantiate(UITraitPrefab, TraitContainer);
            i.Init(t);
        });
    }
}
