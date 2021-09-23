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
    public Transform SpawnSpace;
    public Transform MainCharacterChairs;

    [Header("Data")]
    public List<Sprite> CharacterSprites;
    public Sprite PriestSprite;
    public SO_TraitList AllTraits;
    public SO_SadnessLevelList AllSadnessLevels;
    public float SpawnRadius = 6f;
    public int TraitsPerCharacter = 3;
    public int FriendsPerCharacter = 3;
    public LayerMask CharacterLayerMask;
    public LayerMask ChairLayerMask;
    public int PointsToSubstractPerRandomGroup = 1;
    public int MaxValueToAddSaddness = 0;

    [Header("Realtime data")]
    public List<Character> AllCharactersInScene;
    public List<Chair> AllChairsInScene;

    private List<string> availableNames;
    private List<Sprite> availableSprites;
    private List<SO_Trait> availableTraits;
    
    private UnionStates currentState;
    private Camera mc;
    private Character grabbedCharacter;
    private bool isGrabbingCharacter;
    private Group currentGroup;

    private List<Group> chairGroups;

    private void Awake()
    {
        mc = Camera.main;
        AllChairsInScene = FindObjectsOfType<Chair>().ToList();
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

                    chair.AssignedCharacter = grabbedCharacter;
                    grabbedCharacter.AssignedChair = chair;
                    grabbedCharacter.transform.position = chair.GetCharacterPosition();
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
        currentGroup = group;

        if (group.Characters.Count < 1)
        {
            currentGroup.Characters.Add(GenerateCharacter(CharactersContainer));
            currentGroup.Characters.Add(GenerateCharacter(CharactersContainer));
        }
        else if (group.Characters.Count < 2)
        {
            currentGroup.Characters.Add(GenerateCharacter(CharactersContainer));
        }

        foreach (var cha in currentGroup.Characters)
        {
            if (cha.Friends.Count == 0)
            {
                cha.Friends = GenerateCharacterFriends(cha, FriendsPerCharacter, CharactersContainer);
            }
        }

        // Coloca a todos los personajes en una posición aleatoria
        foreach (var cha in AllCharactersInScene)
        {
            cha.transform.position = GetRandomSpawnPosition();
        }
        
        // Setea los personajes principales para evitar que se puedan mover
        foreach (var cha in group.Characters)
        {
            cha.IsMainCharacter = true;
        }

        StartCoroutine(StartFeast());
    }

    private IEnumerator StartFeast()
    {
        currentState = UnionStates.PreparingFeast;

        // Coloca los personajes principales en sus sillas
        for (var i = 0; i < currentGroup.Characters.Count; i++)
        {
            var cha = currentGroup.Characters[i];

            cha.transform.position = MainCharacterChairs.GetChild(i).position;
        }
        
        yield return new WaitUntil(() => currentState == UnionStates.Feasting); // Se espera a que empiece el festin

        chairGroups = CreateChairGroups(AllChairsInScene);
        
        // TODO: Se desarrolla el festin (Animaciones, efectos, etc)
        
        yield return new WaitUntil(() => true); // TODO: Se espera a se termine el festin

        chairGroups = chairGroups.OrderByDescending(x => x.Value).ToList();

        // Elige un grupo de entre los que más puntos tienen
        var winnerGroups = chairGroups.FindAll(gp => gp.Value == chairGroups[0].Value);
        var chosenGroup = winnerGroups[Random.Range(0, winnerGroups.Count)];

        // Resta puntos a los grupos por ser generados de forma aleatoria
        var randomlyGeneratedGroups = chairGroups.FindAll(gp => gp.RandomlyGenerated);
        randomlyGeneratedGroups.ForEach(gp => gp.Value -= PointsToSubstractPerRandomGroup);
        
        // Añade puntos de tristeza
        var sadGroups = chairGroups.FindAll(gp => gp.Value <= MaxValueToAddSaddness);
        sadGroups.ForEach(gp => gp.Characters.ForEach(c => c.SadnessPoints++));
        
        // TODO: Animaciones en las que los personajes full tristones se convierten en curas
        var extremelySadGuests = AllCharactersInScene.FindAll(c => Utils.CalculateSadness(AllSadnessLevels.List, c).SadnessLevel == SadnessLevel.Extreme);
        extremelySadGuests.ForEach(g => g.SwitchSprite(PriestSprite));

        yield return new WaitUntil(() => currentState == UnionStates.Ending); // TODO: Se espera a que haya que continuar con la siguiente boda
        
        StartUnion(chosenGroup);
    }

    private void Initialize()
    {
        availableNames = Utils.GetAllAvailableNames();
        availableSprites = new List<Sprite>(CharacterSprites);
        availableTraits = new List<SO_Trait>(AllTraits.List);

        currentState = UnionStates.Starting;

        foreach (var chair in AllChairsInScene)
        {
            chair.AssignedCharacter = null;
        }

        foreach (var character in AllCharactersInScene)
        {
            character.AssignedChair = null;
            character.IsMainCharacter = false;
            character.PlacedRandomly = false;
        }
    }
    private List<Group> CreateChairGroups(List<Chair> allChairs)
    {
        var groups = new List<Group>();

        foreach (var ch in allChairs)
        {
            var characters = new List<Character> {ch.AssignedCharacter};

            var exists = false;

            foreach (var gp in groups)
            {
                if (gp.Characters.Contains(ch.AssignedCharacter))
                {
                    exists = true;
                    break;
                }
            }
            
            if (ch.AssignedCharacter == null || exists) continue;
            
            foreach (var lCh in ch.LinkedChairs)
            {
                if (lCh.AssignedCharacter == null) continue;
                characters.Add(lCh.AssignedCharacter);
            }
            
            groups.Add(new Group(characters));
        }

        return groups;
    }
    private Character GenerateCharacter(Transform parent)
    {
        var ch = Instantiate(CharacterPrefab, parent);
        AllCharactersInScene.Add(ch);
        
        var newName = availableNames[Random.Range(0, availableNames.Count)];
        availableNames.Remove(newName);
        
        var newSprite = availableSprites[Random.Range(0, availableSprites.Count)];
        availableSprites.Remove(newSprite);

        var newTraits = GetRandomTraits(availableTraits, TraitsPerCharacter);
        var newFriendsList = new List<Character>();
        
        ch.Init(newName, newSprite, newTraits, newFriendsList);

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
            var ch = GenerateCharacter(parent);
            list.Add(ch);
        }

        parent.gameObject.name = "Friends of " + originalCharacter.CharacterName;

        return list;
    }
    private Vector3 GetRandomSpawnPosition()
    {
        return SpawnSpace.position + (Vector3)Random.insideUnitCircle * SpawnRadius;
    }
    private void PlaceRemainingGuestsInRandomChairs()
    {
        var guestsWithoutChairs = AllCharactersInScene.FindAll(x => x.AssignedChair == null && !x.IsMainCharacter);
        
        foreach (var guest in guestsWithoutChairs)
        {
            var emptyChairs = AllChairsInScene.FindAll(x => x.AssignedCharacter == null);
            guest.PlacedRandomly = true;

            if (emptyChairs.Count == 0) break;
            
            var chair = emptyChairs[Random.Range(0, emptyChairs.Count)];

            chair.AssignedCharacter = guest;
            guest.AssignedChair = chair;

            guest.transform.position = chair.GetCharacterPosition();
        }

        // TODO: Ver qué hacer con los invitados que no tienen silla cuando comienza el festin, por el momento se destruyen
        guestsWithoutChairs = AllCharactersInScene.FindAll(x => x.AssignedChair == null && !x.IsMainCharacter);
        foreach (var guest in guestsWithoutChairs)
        {
            DestroyImmediate(guest.gameObject);
        }
        
        AllCharactersInScene.RemoveAll(item => item == null);

        foreach (var character in AllCharactersInScene)
        {
            character.Friends.RemoveAll(item => item == null);
        }
    }
    
    // BOTONES
    public void ButtonStartFeast()
    {
        currentState = UnionStates.Feasting;
        
        PlaceRemainingGuestsInRandomChairs();
    }
    public void ButtonEndUnion()
    {
        currentState = UnionStates.Ending;
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
