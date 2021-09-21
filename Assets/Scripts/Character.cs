using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer Visual;

    [Header("Data")]
    public bool IsMainCharacter;
    public string CharacterName;
    public List<SO_Trait> Traits;
    public List<Character> Friends;

    public void Init(bool isMainCharacter, string newName, Sprite newSprite, List<SO_Trait> newTraits, List<Character> newFriendsList)
    {
        IsMainCharacter = isMainCharacter;
        CharacterName = newName;
        Visual.sprite = newSprite;
        Traits = newTraits;
        Friends = newFriendsList;

        gameObject.name = "Character - " + CharacterName;
    }
}
