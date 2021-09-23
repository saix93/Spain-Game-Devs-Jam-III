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
    public Transform GuestChairs;

    [Header("Data")]
    public int MinGuests = 4;
    public int MaxGuests = 8;
    public SO_CharacterSpriteList CharacterSprites;
    public Sprite PriestSprite;
    public SO_TraitList AllTraits;
    public SO_SadnessLevelList AllSadnessLevels;
    public float SpawnRadius = 6f;
    public int TraitsPerCharacter = 3;
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
    private Vector3 grabbedCharacterOriginalPosition;
    private Character grabbedCharacter;
    private bool isGrabbingCharacter;
    private Group currentGroup;

    private List<Group> chairGroups;

    public static GameManager _;

    private void Awake()
    {
        _ = this;
        mc = Camera.main;
        AllChairsInScene = GuestChairs.GetComponentsInChildren<Chair>().ToList();
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

                    if (!grabbedCharacter.IsMainCharacter && !grabbedCharacter.IsPriest)
                    {
                        grabbedCharacterOriginalPosition = grabbedCharacter.transform.position;
                        isGrabbingCharacter = true;
                    }
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

                    if (chair.AssignedCharacter != null)
                    {
                        grabbedCharacter.transform.position = grabbedCharacterOriginalPosition;
                        return;
                    }

                    grabbedCharacter.AssignChair(chair);
                }
                else
                {
                    grabbedCharacter.transform.position = grabbedCharacterOriginalPosition;
                }
            }

            if (isGrabbingCharacter)
            {
                var mPos = mc.ScreenToWorldPoint(Input.mousePosition);
                mPos.z = 0;
                
                grabbedCharacter.transform.position = mPos;
            }
            
            // Cuando todos los invitados esten en posición: currentState = UnionStates.Feasting
            // ¿Esperar X tiempo? TODO: Koraen, haz el timer para comprobar aquí si se ha acabado el tiempo
        }
    }

    private void StartUnion(Group group)
    {
        Initialize();
        currentGroup = group;

        if (group.Characters.Count < 1)
        {
            currentGroup.Characters.Add(GenerateCharacter());
            currentGroup.Characters.Add(GenerateCharacter());
        }
        else if (group.Characters.Count < 2)
        {
            currentGroup.Characters.Add(GenerateCharacter()); // Solo ocurre cuando solo hay un invitado, ya que es el unico grupo a seleccionar
        }

        // Genera los invitados
        var currentGuests = AllCharactersInScene.Count - currentGroup.Characters.Count;
        var priests = AllCharactersInScene.FindAll(c => c.IsPriest);
        var min = Mathf.Max(0, MinGuests - currentGuests + priests.Count); // El minimo de guests se aplica sin contar los priests
        var max = MaxGuests - currentGuests;
        var guestNumber = Random.Range(min, max);
        
        for (var i = 0; i < guestNumber; i++)
        {
            GenerateCharacter();
        }
        
        currentGuests = AllCharactersInScene.Count - currentGroup.Characters.Count;
        priests = AllCharactersInScene.FindAll(c => c.IsPriest);
        Debug.Log($"Generados {guestNumber} invitados. Para un total de {priests.Count} priests. {currentGuests - priests.Count} invitados \"normales\"");

        // Coloca a todos los personajes en una posición aleatoria
        // TODO: Cambiar a una posición aleatoria de entre un preset
        AllCharactersInScene.ForEach(c =>
        {
            if (c.IsPriest) return;
            c.transform.position = GetRandomSpawnPosition();
        });
        
        // Setea los personajes principales para evitar que se puedan mover
        group.Characters.ForEach(c => c.IsMainCharacter = true);

        StartCoroutine(Feast());
    }
    private void Initialize()
    {
        availableNames = Utils.GetAllAvailableNames();
        availableSprites = new List<Sprite>(CharacterSprites.List);
        availableTraits = new List<SO_Trait>(AllTraits.List);

        currentState = UnionStates.Starting;

        AllChairsInScene.ForEach(c => c.AssignedCharacter = null);

        AllCharactersInScene.ForEach(c =>
        {
            c.AssignedChair = null;
            c.IsMainCharacter = false;
            c.PlacedRandomly = false;
        });

        // Asigna todos los priests a sus sillas
        // TODO: Revisar qué tal funciona. También se puede hacer que los priests se queden en los sitios que tenían
        // pero esto puede hacer más injusto el gameplay
        var priests = AllCharactersInScene.FindAll(c => c.IsPriest);
        for (var i = 0; i < priests.Count; i++)
        {
            priests[i].AssignChair(AllChairsInScene[i]);
        }
    }
    private IEnumerator Feast()
    {
        currentState = UnionStates.PreparingFeast;

        // Coloca los personajes principales en sus sillas
        for (var i = 0; i < currentGroup.Characters.Count; i++)
        {
            var cha = currentGroup.Characters[i];

            cha.transform.position = MainCharacterChairs.GetChild(i).position;
        }
        
        yield return new WaitUntil(() => currentState == UnionStates.Feasting); // Se espera a que empiece el festin

        PlaceRemainingGuestsInRandomChairs();
        chairGroups = CreateChairGroups(AllChairsInScene);
        
        // TODO: Se desarrolla el festin (Animaciones, efectos, etc)
        
        yield return new WaitUntil(() => currentState == UnionStates.Ending); // TODO: Se espera a se termine el festin

        // Resta puntos a los grupos por ser generados de forma aleatoria
        var randomlyGeneratedGroups = chairGroups.FindAll(gp => gp.RandomlyGenerated);
        randomlyGeneratedGroups.ForEach(gp => gp.Value -= PointsToSubstractPerRandomGroup);
        
        // Añade puntos de tristeza
        var sadGroups = chairGroups.FindAll(gp => gp.Value <= MaxValueToAddSaddness);
        sadGroups.ForEach(gp => gp.Characters.ForEach(c => c.SadnessPoints++));
        
        // TODO: Animaciones en las que los personajes full tristones se convierten en curas
        var extremelySadGuests = AllCharactersInScene.FindAll(c => Utils.CalculateSadness(c).SadnessLevel == SadnessLevel.Extreme);
        extremelySadGuests.ForEach(g => g.TurnIntoPriest(PriestSprite));
        
        // Reevalua los grupos para evitar que salga ganador un grupo con priest
        chairGroups.ForEach(g => g.ReEvaluateGroupValue());

        // Elige un grupo de entre los que más puntos tienen
        chairGroups = chairGroups.OrderByDescending(x => x.Value).ToList();
        var winnerGroups = chairGroups.FindAll(gp => gp.Value == chairGroups[0].Value);
        var chosenGroup = winnerGroups[Random.Range(0, winnerGroups.Count)];

        yield return new WaitUntil(() => currentState == UnionStates.NextUnion); // TODO: Se espera a que haya que continuar con la siguiente boda
        
        currentGroup.Characters.ForEach(c => DestroyImmediate(c.gameObject));
        AllCharactersInScene.RemoveAll(item => item == null);
        
        // TODO: Añadir una pequeña probabilidad de que algún invitado sea eliminado de la lista
        
        StartUnion(chosenGroup);
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
    private Character GenerateCharacter()
    {
        var ch = Instantiate(CharacterPrefab, CharactersContainer);
        AllCharactersInScene.Add(ch);
        
        var newName = availableNames[Random.Range(0, availableNames.Count)];
        availableNames.Remove(newName);
        
        var newSprite = availableSprites[Random.Range(0, availableSprites.Count)];
        availableSprites.Remove(newSprite);

        var newTraits = GetRandomTraits(availableTraits, TraitsPerCharacter);
        
        ch.Init(newName, newSprite, newTraits);

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
    }
    
    // BOTONES
    public void ButtonStartFeast()
    {
        currentState = UnionStates.Feasting;
    }
    public void ButtonEndUnion()
    {
        currentState = UnionStates.Ending;
    }
    public void ButtonNextUnion()
    {
        currentState = UnionStates.NextUnion;
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
    Ending,
    NextUnion
}
