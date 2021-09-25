using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Character CharacterPrefab;
    public Transform CharactersContainer;
    public Transform SpawnZone;
    public Transform MainCharacterChairs;
    public Transform GuestChairs;

    [Header("Data")]
    public int MinGuests = 4;
    public int MaxGuests = 8;
    public bool AlwaysSpawnMaxGuests = false;
    public SO_CharacterSpriteList CharacterSprites;
    public Sprite PriestSprite;
    public SO_TraitList AllTraits;
    public SO_SadnessLevelList AllSadnessLevels;
    public int TraitsPerCharacter = 3;
    public LayerMask CharacterLayerMask;
    public LayerMask ChairLayerMask;
    public int PointsToSubstractPerRandomGroup = 1;
    public int MaxValueToAddSaddness = 0;
    public int MinFreeChairsToPlay = 4;

    [Header("Animations")]
    public float TimeToMoveRandomCharacters = 1;

    [Header("Realtime data")]
    public List<Character> AllCharactersInScene;
    public List<Chair> AllMainCharacterChairs;
    public List<Chair> AllGuestChairs;
    public List<SpawnPoint> AllSpawnPoints;

    private List<string> availableNames;
    private List<Sprite> availableSprites;
    private List<SO_Trait> availableTraits;
    private List<SpawnPoint> availableSpawnPoints;
    
    private UnionStates currentState;
    private Camera mc;
    private Vector3 grabbedCharacterOriginalPosition;
    private Character grabbedCharacter;
    private bool isGrabbingCharacter;
    private Group currentGroup;

    public static GameManager _;

    private void Awake()
    {
        _ = this;
        mc = Camera.main;
        AllGuestChairs = GuestChairs.GetComponentsInChildren<Chair>().ToList();
        AllMainCharacterChairs = MainCharacterChairs.GetComponentsInChildren<Chair>().ToList();
        
        AllSpawnPoints = new List<SpawnPoint>();
        foreach (Transform child in SpawnZone)
        {
            AllSpawnPoints.Add(child.GetComponent<SpawnPoint>());
        }
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
                        grabbedCharacter.IsBeingGrabbed = true;
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (!isGrabbingCharacter) return;
                isGrabbingCharacter = false;
                grabbedCharacter.IsBeingGrabbed = false;
                
                var mPos = mc.ScreenToWorldPoint(Input.mousePosition);
                mPos.z = 0;
                
                var hit = Physics2D.Raycast(mPos, Vector2.zero, Mathf.Infinity, ChairLayerMask);

                if (hit.collider != null)
                {
                    var chair = hit.collider.GetComponent<Chair>();
                    var spawnPoint = hit.collider.GetComponent<SpawnPoint>();

                    if (chair != null)
                    {
                        if (chair.AssignedCharacter is null)
                        {
                            grabbedCharacter.AssignChair(chair);
                        }
                        else
                        {
                            grabbedCharacter.transform.position = grabbedCharacterOriginalPosition;
                            return;
                        }
                    }
                    else if (spawnPoint != null)
                    {
                        if (spawnPoint.AssignedCharacter is null)
                        {
                            grabbedCharacter.AssignSpawnPoint(spawnPoint);
                        }
                        else
                        {
                            grabbedCharacter.transform.position = grabbedCharacterOriginalPosition;
                            return;
                        }
                    }
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
        if (AlwaysSpawnMaxGuests) guestNumber = max;
        
        for (var i = 0; i < guestNumber; i++)
        {
            GenerateCharacter();
        }
        
//        currentGuests = AllCharactersInScene.Count - currentGroup.Characters.Count;
//        priests = AllCharactersInScene.FindAll(c => c.IsPriest);
//        Debug.Log($"Generados {guestNumber} invitados. Para un total de {priests.Count} priests. {currentGuests - priests.Count} invitados \"normales\"");

        // Coloca a todos los personajes en una posición aleatoria
        // TODO: Cambiar a una posición aleatoria de entre un preset de posiciones
        AllCharactersInScene.ForEach(c =>
        {
            if (c.IsPriest) return;
            var spawnPos = GetRandomSpawnPosition();
            c.AssignSpawnPoint(spawnPos);
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
        availableSpawnPoints = new List<SpawnPoint>(AllSpawnPoints);

        currentState = UnionStates.Starting;

        AllGuestChairs.ForEach(c => c.AssignedCharacter = null);

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
            priests[i].AssignChair(AllGuestChairs[i]);
        }
    }
    private IEnumerator Feast()
    {
        currentState = UnionStates.PreparingFeast;

        // Coloca los personajes principales en sus sillas
        for (var i = 0; i < currentGroup.Characters.Count; i++)
        {
            var cha = currentGroup.Characters[i];
            
            cha.AssignChair(AllMainCharacterChairs[i]);
        }
        
        yield return new WaitUntil(() => currentState == UnionStates.Feasting); // Se espera a que empiece el festin
        
        // Se coloca a los invitados restantes en sillas aleatorias
        var guestsWithoutChairs = AllCharactersInScene.FindAll(x => x.AssignedChair == null && !x.IsMainCharacter);
        var emptyChairs = AllGuestChairs.FindAll(x => x.AssignedCharacter == null);
        
        foreach (var guest in guestsWithoutChairs)
        {
            guest.PlacedRandomly = true;
            
            var chair = emptyChairs[Random.Range(0, emptyChairs.Count)];
            emptyChairs.Remove(chair);

            var initialPos = guest.transform.position;
            var finalPos = chair.GetCharacterPosition();
            var factor = 0f;

            do
            {
                var currentPos = Vector3.Lerp(initialPos, finalPos, factor);
                factor += Time.deltaTime / TimeToMoveRandomCharacters;

                guest.transform.position = currentPos;
                
                yield return null;
            } while (Vector3.Distance(guest.transform.position, finalPos) > .1f);

            guest.AssignChair(chair);
        }

        // TODO: Se desarrolla el festin (Animaciones, efectos, etc)
        
        yield return new WaitUntil(() => currentState == UnionStates.Ending);

        var allChairGroups = CreateChairGroups(AllGuestChairs);
        
        // Resta puntos a los grupos por ser generados de forma aleatoria
        var randomlyGeneratedGroups = allChairGroups.FindAll(gp => gp.RandomlyGenerated);
        randomlyGeneratedGroups.ForEach(gp => gp.Value -= PointsToSubstractPerRandomGroup);
        
        // Añade puntos de tristeza
        var sadGroups = allChairGroups.FindAll(gp => gp.Value <= MaxValueToAddSaddness);
        sadGroups.ForEach(gp => gp.Characters.ForEach(c => c.SadnessPoints++));
        
        // TODO: Animaciones en las que los personajes full tristones se convierten en curas
        var extremelySadGuests = AllCharactersInScene.FindAll(c => Utils.CalculateSadness(c).SadnessLevel == SadnessLevel.Extreme);
        extremelySadGuests.ForEach(g => g.TurnIntoPriest(PriestSprite));
        
        // Reevalua los grupos para evitar que salga ganador un grupo con priest
        allChairGroups.ForEach(g => g.ReEvaluateGroupValue());

        // Elige un grupo de entre los que más puntos tienen
        allChairGroups = allChairGroups.OrderByDescending(x => x.Value).ToList();
        var winnerGroups = allChairGroups.FindAll(gp => gp.Value == allChairGroups[0].Value);
        var chosenGroup = winnerGroups[Random.Range(0, winnerGroups.Count)];

        yield return new WaitUntil(() => currentState == UnionStates.NextUnion); // TODO: Se espera a que haya que continuar con la siguiente boda
        
        currentGroup.Characters.ForEach(c => DestroyImmediate(c.gameObject));
        AllCharactersInScene.RemoveAll(item => item == null);

        // Losing conditions -> número total de sillas - priests <= minimo de sillas libres || todos los grupos tienen al menos un priest
        var freeChairs = AllGuestChairs.Count - AllCharactersInScene.FindAll(c => c.IsPriest).Count;
        if (freeChairs <= MinFreeChairsToPlay || allChairGroups.All(g => g.HasPriest))
        {
            EndGame();
        }
        else
        {
            StartUnion(chosenGroup);
        }
    }
    private void EndGame()
    {
        // TODO: Fin del juego
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
            
            if (ch.AssignedCharacter is null || exists) continue;
            
            foreach (var lCh in ch.LinkedChairs)
            {
                if (lCh.AssignedCharacter is null) continue;
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
    private SpawnPoint GetRandomSpawnPosition()
    {
        var spawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];
        availableSpawnPoints.Remove(spawnPoint);

        return spawnPoint;
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
    public void ButtonResetEverything()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ButtonAdvancePhase()
    {
        currentState += 1;
        currentState = (UnionStates)Mathf.Min((int)currentState, (int) Enum.GetValues(typeof(UnionStates)).Cast<UnionStates>().Last());
    }
    
    // EXTRA
    public UnionStates GetCurrentState()
    {
        return currentState;
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
