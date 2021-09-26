using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Character : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer Visual;
    public UI_Character UICharacter;

    [Header("Data")]
    public MinMaxInt OrderInLayer = new MinMaxInt(200, 2000);
    public MinMaxInt PositionY = new MinMaxInt(-4, 18);

    [Header("Animations")]
    public Sprite SadIcon;
    public Sprite PriestIcon;

    [Header("Realtime data")]
    public bool IsMainCharacter;
    public string CharacterName;
    public List<SO_Trait> Traits;
    public Chair AssignedChair;
    public SpawnPoint AssignedSpawnPoint;
    public bool IsBeingGrabbed;
    public int SadnessPoints;
    public bool PlacedRandomly;
    public bool IsPriest;

    private void Update()
    {
        var factor = Mathf.InverseLerp(PositionY.Min, PositionY.Max, transform.position.y);
        var order = (int)Mathf.Lerp(OrderInLayer.Max, OrderInLayer.Min, factor);

        ChangeOrderInLayer(IsBeingGrabbed ? 32767 : order);
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

    public void ShowEmote(Group group)
    {
        if (group.Characters.Count > 1)
        {
            // REVISAR: Esto tiene un problema, si tienen 2 o más traits en común y los traits no están ordenados de la misma forma
            // puede ocurrir que uno de los personajes del grupo muestre un trait y el otro personaje muestre otro
            var other = group.Characters.Find(c => c != this);
            var icon = GetRandomCommonTrait(other).Icon;
            
            if (!icon) icon = Traits[Random.Range(0, Traits.Count)].Icon;

            UICharacter.ShowEmote(IsPriest ? PriestIcon : icon);
        }
        else
        {
            UICharacter.ShowEmote(IsPriest ? PriestIcon : SadIcon);
        }
    }
    private SO_Trait GetRandomCommonTrait(Character other)
    {
        foreach (var t in Traits)
        {
            if (other.Traits.Contains(t)) return t;
        }

        return null;
    }

    public void AssignSpawnPoint(SpawnPoint sp)
    {
        ClearAssignedChair();
        if (AssignedSpawnPoint) AssignedSpawnPoint.AssignedCharacter = null;
        AssignedSpawnPoint = sp;
        sp.AssignedCharacter = this;
        transform.position = sp.GetCharacterPosition();
    }
    public void AssignChair(Chair chair)
    {
        ClearAssignedSpawnPoint();
        if (AssignedChair) AssignedChair.AssignedCharacter = null;
        AssignedChair = chair;
        chair.AssignedCharacter = this;
        transform.position = chair.GetCharacterPosition();
    }

    private void ClearAssignedChair()
    {
        if (!AssignedChair) return;
        
        AssignedChair.AssignedCharacter = null;
        AssignedChair = null;
    }
    private void ClearAssignedSpawnPoint()
    {
        if (!AssignedSpawnPoint) return;
        
        AssignedSpawnPoint.AssignedCharacter = null;
        AssignedSpawnPoint = null;
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
        UICharacter.ChangeOrderInLayer(newOrder);
    }
}
