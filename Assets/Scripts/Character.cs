using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer Visual;

    [Header("Data")]
    public string CharacterName;
    public List<SO_Trait> Traits;
    public List<Character> Friends;

    public void Init(string newName, Sprite newSprite, List<Character> newFriendsList)
    {
        CharacterName = newName;
        Visual.sprite = newSprite;
        Friends = newFriendsList;

        gameObject.name = "Character - " + CharacterName;
    }
}
