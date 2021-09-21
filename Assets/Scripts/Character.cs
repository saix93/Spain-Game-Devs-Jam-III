using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer CharacterVisual;

    [Header("Data")]
    public string CharacterName;
    public List<Character> Friends;

    public void Init(string newName, Sprite newSprite, List<Character> newFriendsList)
    {
        CharacterName = newName;
        CharacterVisual.sprite = newSprite;
        Friends = newFriendsList;

        gameObject.name = "Character - " + CharacterName;
    }
}
