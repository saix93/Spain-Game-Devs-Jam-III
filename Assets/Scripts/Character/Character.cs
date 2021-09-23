using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer Visual;
    public UI_Character UICharacter;

    [Header("Realtime data")]
    public bool IsMainCharacter;
    public string CharacterName;
    public List<SO_Trait> Traits;
    public Chair AssignedChair;
    public int SadnessPoints;
    public bool PlacedRandomly;
    public bool IsPriest;

    public void Init(string newName, Sprite newSprite, List<SO_Trait> newTraits)
    {
        IsMainCharacter = false;
        PlacedRandomly = false;
        IsPriest = false;
        CharacterName = newName;
        Visual.sprite = newSprite;
        Traits = newTraits;

        gameObject.name = "Character - " + CharacterName;
        
        UICharacter.Init(this);
    }

    public void AssignChair(Chair chair)
    {
        if (!(AssignedChair is null)) AssignedChair.AssignedCharacter = null;
        AssignedChair = chair;
        chair.AssignedCharacter = this;
        transform.position = chair.GetCharacterPosition();
    }

    public void TurnIntoPriest(Sprite newSprite)
    {
        SwitchSprite(newSprite);
        IsPriest = true;
    }

    private void SwitchSprite(Sprite newSprite)
    {
        Visual.sprite = newSprite;
    }
}
