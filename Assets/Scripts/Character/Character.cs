using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer Visual;
    public UI_Character UICharacter;

    [Header("Data")]
    public MinMaxInt OrderInLayer = new MinMaxInt(200, 2000);
    public MinMaxInt PositionY = new MinMaxInt(-4, 18);

    [Header("Realtime data")]
    public bool IsMainCharacter;
    public string CharacterName;
    public List<SO_Trait> Traits;
    public Chair AssignedChair;
    public bool IsBeingGrabbed;
    public int SadnessPoints;
    public bool PlacedRandomly;
    public bool IsPriest;

    private void Update()
    {
        var factor = Mathf.InverseLerp(PositionY.Min, PositionY.Max, transform.position.y);
        var order = (int)Mathf.Lerp(OrderInLayer.Max, OrderInLayer.Min, factor);

        ChangeOrderInLayer(IsBeingGrabbed ? 99999 : order);
    }

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

    private void ChangeOrderInLayer(int newOrder)
    {
        Visual.sortingOrder = newOrder;
    }
}
