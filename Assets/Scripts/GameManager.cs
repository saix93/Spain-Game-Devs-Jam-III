using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public Transform Character1Chair;
    public Transform Character2Chair;

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

    private List<Group> chairGroups;

    private void Awake()
    {
        mc = Camera.main;
    }

    private void Start()
    {
        var gp = new Group();
        StartUnion(gp);
    }

    private void Update()
    {
        if (currentState == UnionStates.PreparingFeast)
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
                    
                    if (!grabbedCharacter.IsMainCharacter) isGrabbingCharacter = true;
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
                    var chair = hit.collider.GetComponent<Chair>();

                    chair.SittingCharacter = grabbedCharacter;
                    grabbedCharacter.transform.position = chair.transform.position + (Vector3)chair.CharacterSitPositionOffset;
                }
            }

            if (isGrabbingCharacter)
            {
                var mPos = mc.ScreenToWorldPoint(Input.mousePosition);
                mPos.z = 0;
                
                grabbedCharacter.transform.position = mPos;
            }
            
            // Cuando todos los invitados esten en posición: currentState = UnionStates.Feasting
            // ¿Esperar X tiempo?
        }
    }

    private void StartUnion(Group group)
    {
        Initialize();

        if (group.Characters.Count < 1)
        {
            character1 = GenerateCharacter(true, CharactersContainer);
            character2 = GenerateCharacter(true, CharactersContainer);
        }
        else if (group.Characters.Count < 2)
        {
            character1 = group.Characters[0];
            character2 = GenerateCharacter(true, CharactersContainer);
        }
        else
        {
            character1 = group.Characters[0];
            character2 = group.Characters[0];
        }

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
        currentState = UnionStates.PreparingFeast;

        character1.transform.position = Character1Chair.position;
        character2.transform.position = Character2Chair.position;
        
        yield return new WaitUntil(() => currentState == UnionStates.Feasting); // Se espera a que empiece el festin

        var allChairs = FindObjectsOfType<Chair>();
        chairGroups = CreateChairGroups(allChairs);
        
        // TODO: Se desarrolla el festin (Animaciones, efectos, etc)
        
        yield return new WaitUntil(() => true); // TODO: Se espera a se termine el festin

        chairGroups = chairGroups.OrderBy(x => x.Value).ToList();

        var winnerGroups = chairGroups.FindAll(gp => gp.Value == chairGroups[0].Value);
        var chosenGroup = winnerGroups[Random.Range(0, winnerGroups.Count)];

        currentState = UnionStates.Ending;

        yield return new WaitUntil(() => true); // TODO: Se espera a que haya que continuara la siguiente boda
        
        StartUnion(chosenGroup);
    }
    
    private List<Group> CreateChairGroups(Chair[] allChairs)
    {
        var groups = new List<Group>();

        foreach (var ch in allChairs)
        {
            var characters = new List<Character> {ch.SittingCharacter};

            var exists = false;

            foreach (var gp in groups)
            {
                if (gp.Characters.Contains(ch.SittingCharacter))
                {
                    exists = true;
                    break;
                }
            }
            
            if (ch.SittingCharacter == null || exists) continue;
            
            foreach (var lCh in ch.LinkedChairs)
            {
                characters.Add(lCh.SittingCharacter);
            }
            
            groups.Add(new Group(characters));
        }

        return groups;
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
    
    // BOTONES
    public void ButtonStartFeast()
    {
        currentState = UnionStates.Feasting;
        
        // TODO: Hacer que los personajes que no están sentados en ninguna silla se sienten en una random
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
    PreparingFeast,
    Feasting,
    Ending
}
