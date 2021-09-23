using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer Visual;
    
    [Header("Realtime data")]
    public bool IsMainCharacter;
    public string CharacterName;
    public List<SO_Trait> Traits;
    public List<Character> Friends;
    public Chair AssignedChair;
    public int SadnessPoints;
    public bool PlacedRandomly;

    public void Init(string newName, Sprite newSprite, List<SO_Trait> newTraits, List<Character> newFriendsList)
    {
        IsMainCharacter = false;
        PlacedRandomly = false;
        CharacterName = newName;
        Visual.sprite = newSprite;
        Traits = newTraits;
        Friends = newFriendsList;

        gameObject.name = "Character - " + CharacterName;
    }

    public void SwitchSprite(Sprite newSprite)
    {
        Visual.sprite = newSprite;
    }
}
