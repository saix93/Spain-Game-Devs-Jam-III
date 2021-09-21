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
    public SO_TraitList AllTraits;
    public float SpawnRadius = 6f;
    public int TraitsPerCharacter = 3;
    public LayerMask CharacterLayerMask;
    public LayerMask ChairLayerMask;

    private List<string> availableNames;
    private List<Sprite> availableSprites;
    private List<SO_Trait> availableTraits;
    private Character character1;
    private Character character2;
    private UnionStates currentState;

    private Camera mc;
    private Character grabbedCharacter;
    private bool isGrabbingCharacter;

    private void Awake()
    {
        mc = Camera.main;
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
            if (Input.GetMouseButtonDown(0)) // Left click
            {
                var mPos = mc.ScreenToWorldPoint(Input.mousePosition);
                mPos.z = 0;
                
                var hit = Physics2D.Raycast(mPos, Vector2.zero, Mathf.Infinity, CharacterLayerMask);

                if (hit.collider != null)
                {
                    grabbedCharacter = hit.collider.GetComponent<Character>();
                    isGrabbingCharacter = true;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (!isGrabbingCharacter) return;
                isGrabbingCharacter = false;
                
                var mPos = mc.ScreenToWorldPoint(Input.mousePosition);
                mPos.z = 0;
                
                var hit = Physics2D.Raycast(mPos, Vector2.zero, Mathf.Infinity, ChairLayerMask);

                if (hit.collider != null)
                {
                    grabbedCharacter.transform.position = hit.transform.position;
                }
            }

            if (isGrabbingCharacter)
            {
                var mPos = mc.ScreenToWorldPoint(Input.mousePosition);
                mPos.z = 0;
                
                grabbedCharacter.transform.position = mPos;
            }
            
            // Cuando todos los invitados esten en posición: currentState = UnionStates.Feast
        }
    }

    private void StartUnion(Character ch1, Character ch2)
    {
        Initialize();
        
        character1 = ch1 == null ? GenerateCharacter(true, CharactersContainer) : ch1;
        character2 = ch2 == null ? GenerateCharacter(true, CharactersContainer) : ch2;

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

    private void Initialize()
    {
        availableNames = Utils.GetAllAvailableNames();
        availableSprites = new List<Sprite>(CharacterSprites);
        availableTraits = new List<SO_Trait>(AllTraits.List);

        currentState = UnionStates.Starting;
    }

    private IEnumerator StartFeast()
    {
        yield return new WaitUntil(() => currentState == UnionStates.Feast);
        
        // TODO: Se desarrolla el banquete
        
        yield return new WaitUntil(() => true);
        
        // TODO: Se termina el banquete, hay que determinar cual es la siguiente pareja
        // currentStatus = UnionStates.Ending
    }

    private Character GenerateCharacter(bool isMainCharacter, Transform parent)
    {
        var position = SpawnSpace.position + (Vector3)Random.insideUnitCircle * SpawnRadius;
        var ch = Instantiate(CharacterPrefab, position, Quaternion.identity, parent);
        
        var newName = availableNames[Random.Range(0, availableNames.Count)];
        availableNames.Remove(newName);
        
        var newSprite = availableSprites[Random.Range(0, availableSprites.Count)];
        availableSprites.Remove(newSprite);

        var newTraits = GetRandomTraits(availableTraits, TraitsPerCharacter);
        var newFriendsList = new List<Character>();
        
        ch.Init(isMainCharacter, newName, newSprite, newTraits, newFriendsList);

        return ch;
    }

    private List<SO_Trait> GetRandomTraits(List<SO_Trait> traitList, int num)
    {
        var list = new List<SO_Trait>();

        var indices = Utils.GetDifferentRandomNumbers(0, traitList.Count, num);
        foreach (var index in indices)
        {
            list.Add(traitList[index]);
        }

        return list;
    }

    private List<Character> GenerateCharacterFriends(Character originalCharacter, int itemNumber, Transform parent)
    {
        var list = new List<Character>();
        
        for (var i = 0; i < itemNumber; i++)
        {
            var ch = GenerateCharacter(false, parent);
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
