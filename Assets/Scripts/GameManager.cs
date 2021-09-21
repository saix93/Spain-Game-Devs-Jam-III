using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Character CharacterPrefab;
    public Transform CharactersContainer;
    public Transform FriendsOfFirstCharacterContainer;
    public Transform FriendsOfSecondCharacterContainer;

    [Header("Data")]
    public List<Sprite> CharacterSprites;

    private Character character1;
    private Character character2;

    private List<string> availableCharacterNames;
    private List<Sprite> availableCharacterSprites;

    private void Initialize()
    {
        availableCharacterNames = Utils.GetAllAvailableNames();
        availableCharacterSprites = new List<Sprite>(CharacterSprites);
    }

    private void Start()
    {
        StartUnion(null, null);
    }

    private void StartUnion(Character ch1, Character ch2)
    {
        Initialize();
        
        character1 = ch1 == null ? GenerateCharacter(CharactersContainer) : ch1;
        character2 = ch2 == null ? GenerateCharacter(CharactersContainer) : ch2;

        if (character1.Friends.Count == 0)
        {
            character1.Friends = GenerateCharacterFriends(character1, 2, FriendsOfFirstCharacterContainer);
        }

        if (character2.Friends.Count == 0)
        {
            character2.Friends = GenerateCharacterFriends(character2, 2, FriendsOfSecondCharacterContainer);
        }
    }

    private Character GenerateCharacter(Transform parent)
    {
        var ch = Instantiate(CharacterPrefab, parent);

        var newName = availableCharacterNames[Random.Range(0, availableCharacterNames.Count)];
        availableCharacterNames.Remove(newName);

        var newSprite = availableCharacterSprites[Random.Range(0, availableCharacterSprites.Count)];
        availableCharacterSprites.Remove(newSprite);
        
        var newFriendsList = new List<Character>();
        
        ch.Init(newName, newSprite, newFriendsList);

        return ch;
    }

    private List<Character> GenerateCharacterFriends(Character originalCharacter, int itemNumber, Transform parent)
    {
        var list = new List<Character>();
        
        for (var i = 0; i < itemNumber; i++)
        {
            var ch = GenerateCharacter(parent);
            list.Add(ch);
        }

        parent.gameObject.name = "Friends of " + originalCharacter.CharacterName;

        return list;
    }
}
