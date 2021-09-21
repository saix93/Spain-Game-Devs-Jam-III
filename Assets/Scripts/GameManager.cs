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
    public Transform SpawnSpace;

    [Header("Data")]
    public List<Sprite> CharacterSprites;
    public float SpawnRadius = 6f;

    private Character character1;
    private Character character2;

    private List<string> availableCharacterNames;
    private List<Sprite> availableCharacterSprites;

    private UnionStates currentState;

    private void Initialize()
    {
        availableCharacterNames = Utils.GetAllAvailableNames();
        availableCharacterSprites = new List<Sprite>(CharacterSprites);

        currentState = UnionStates.Starting;
    }

    private void Start()
    {
        StartUnion(null, null);
    }

    private void Update()
    {
        if (currentState == UnionStates.Starting)
        {
            // TODO: Se puede mover a los invitados de posición
        }
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

        StartCoroutine(StartFeast());
    }

    private IEnumerator StartFeast()
    {
        yield return new WaitUntil(() => currentState == UnionStates.Feast);
        
        // TODO: Se desarrolla el banquete
    }

    private Character GenerateCharacter(Transform parent)
    {
        var position = SpawnSpace.position + (Vector3)Random.insideUnitCircle * SpawnRadius;
        
        var ch = Instantiate(CharacterPrefab, position, Quaternion.identity, parent);

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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(SpawnSpace.position, SpawnRadius);
    }
}

public enum UnionStates
{
    Starting,
    Feast,
    Ending
}
