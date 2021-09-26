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
    public Camera MainCamera;
    public Camera FeastCamera;
    public GameObject FadeCircle;
    public Transform FadeMask;

    [Header("Data")]
    public MinMaxInt GuestsNumber = new MinMaxInt(4, 8);
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
    public float TimeToEndUnion = 2f;
    public float TimeBetweenUnions = 2f;

    [Header("Animations")]
    public float TimeToMoveRandomCharacters = 1;
    public float TimeToTurnCharacterIntoPriest = 1;
    public float FadeScaleTime = 1;
    public float FadeAnimationWaitTime = 1;
    public MinMaxFloat RandomTimeBetweenReactions = new MinMaxFloat(.5f, 1.5f);
    public Sprite HeartIcon;

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
        var min = Mathf.Max(0, GuestsNumber.Min - currentGuests + priests.Count); // El minimo de guests se aplica sin contar los priests
        var max = GuestsNumber.Max - currentGuests;
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
        // Reinicia las cámaras
        MainCamera.gameObject.SetActive(true);
        FeastCamera.gameObject.SetActive(false);
        
        availableNames = Utils.GetAllAvailableNames();
        availableSprites = new List<Sprite>(CharacterSprites.List);
        availableTraits = new List<SO_Trait>(AllTraits.List);
        SpawnZone.gameObject.SetActive(true);
        availableSpawnPoints = new List<SpawnPoint>(AllSpawnPoints);

        currentState = UnionStates.Starting;

        AllGuestChairs.ForEach(c => c.AssignedCharacter = null);

        AllCharactersInScene.ForEach(c =>
        {
            c.AssignedChair = null;
            c.IsMainCharacter = false;
            c.PlacedRandomly = false;
            c.EmoteShown = false;
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
        // Coloca los personajes principales en sus sillas
        for (var i = 0; i < currentGroup.Characters.Count; i++)
        {
            var cha = currentGroup.Characters[i];
            
            cha.AssignChair(AllMainCharacterChairs[i]);
        }

        yield return StartCoroutine(FadeAnimation(currentGroup.GetMiddlePosition(), 1));
        FadeCircle.SetActive(false);
        
        currentState = UnionStates.PreparingFeast;
        
        yield return new WaitUntil(() => currentState == UnionStates.Feasting); // Se espera a que empiece el festin
        
        // Cambia la cámara a la del festin
        MainCamera.gameObject.SetActive(false);
        FeastCamera.gameObject.SetActive(true);
        
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
        
        // Se ocultan los SpawnPoints
        SpawnZone.gameObject.SetActive(false);
        
        // Crea los grupos de sillas, la posición de los invitados es final
        var allChairGroups = CreateChairGroups(AllGuestChairs);

        var guests = AllCharactersInScene.FindAll(c => !c.IsMainCharacter);
        foreach (var guest in guests)
        {
            if (guest.EmoteShown) continue;
            
            var reactionTime = Random.Range(RandomTimeBetweenReactions.Min, RandomTimeBetweenReactions.Max);

            yield return new WaitForSeconds(reactionTime);

            var guestGroup = allChairGroups.Find(g => g.Characters.Contains(guest));
            guest.ShowEmote(guestGroup);
        }
        
        yield return new WaitForSeconds(TimeToEndUnion); // Termina la fase de banquete y comienza la fase final, que termina la boda

        // Resta puntos a los grupos por ser generados de forma aleatoria
        var randomlyGeneratedGroups = allChairGroups.FindAll(gp => gp.RandomlyGenerated);
        randomlyGeneratedGroups.ForEach(gp => gp.Value -= PointsToSubstractPerRandomGroup);
        
        // Añade puntos de tristeza
        var sadGroups = allChairGroups.FindAll(gp => gp.Value <= MaxValueToAddSaddness);
        sadGroups.ForEach(gp => gp.Characters.ForEach(c => c.SadnessPoints++));
        
        var extremelySadGuests = AllCharactersInScene.FindAll(c => Utils.CalculateSadness(c).SadnessLevel == SadnessLevel.Extreme);
        foreach (var guest in extremelySadGuests)
        {
            yield return StartCoroutine(PriestAnimation(guest));
        }
        
        // Reevalua los grupos para evitar que salga ganador un grupo con priest
        allChairGroups.ForEach(g => g.ReEvaluateGroupValue());

        // Elige un grupo de entre los que más puntos tienen
        allChairGroups = allChairGroups.OrderByDescending(x => x.Value).ToList();
        var winnerGroups = allChairGroups.FindAll(gp => gp.Value == allChairGroups[0].Value);
        var chosenGroup = winnerGroups[Random.Range(0, winnerGroups.Count)];
        
        // Muestra el emote del corazón en la nueva pareja
        chosenGroup.Characters.ForEach(c => c.ShowSpecificEmote(HeartIcon));
        yield return StartCoroutine(FadeAnimation(chosenGroup.GetMiddlePosition(), -1));
        
        currentGroup.Characters.ForEach(c => DestroyImmediate(c.gameObject));
        AllCharactersInScene.RemoveAll(item => item == null);
        
        // Esperamos a empezar la siguiente boda
        yield return new WaitForSeconds(TimeBetweenUnions);
        
        if (EvaluateLoseConditions(allChairGroups).Any(b => b))
        {
            EndGame();
        }
        else
        {
            StartUnion(chosenGroup);
        }
    }
    private IEnumerator FadeAnimation(Vector3 targetPosition, int direction)
    {
        FadeCircle.SetActive(true);
        FadeMask.position = targetPosition;
        var targetScale = direction > 0 ? Vector3.one : Vector3.zero;
        
        FadeMask.localScale = direction > 0 ? Vector3.zero : Vector3.one;
        var scaleFactor = FadeMask.localScale.normalized.magnitude;
        bool haveToWaitForAnimation = true;
        do
        {
            scaleFactor += (Time.deltaTime / FadeScaleTime) * direction;
            FadeMask.localScale = Vector3.one * scaleFactor;
            
            if (haveToWaitForAnimation && Vector3.Distance(Vector3.one * .1f, FadeMask.localScale) < .01f)
            {
                haveToWaitForAnimation = false;
                yield return new WaitForSeconds(FadeAnimationWaitTime);
            }

            yield return null;
        } while (Vector3.Distance(targetScale, FadeMask.localScale) > .01f);

        FadeMask.localScale = targetScale;
    }
    private IEnumerator PriestAnimation(Character character)
    {
        // TODO: Mejorar. Tremenda chapuza
        character.Visual.material = new Material(character.Visual.material);
        var factor = 0f;

        do
        {
            factor -= Time.deltaTime * TimeToTurnCharacterIntoPriest;
            character.UpdateAlpha(factor);

            yield return null;
        } while (character.Visual.material.GetVector("_HSVAAdjust").w > -1f);
        
        character.TurnIntoPriest(PriestSprite);

        factor = -1f;

        do
        {
            factor += Time.deltaTime * TimeToTurnCharacterIntoPriest;
            character.UpdateAlpha(factor);

            yield return null;
        } while (character.Visual.material.GetVector("_HSVAAdjust").w < 0f);
    }

    private List<bool> EvaluateLoseConditions(List<Group> allGroups)
    {
        var list = new List<bool>();
        var freeChairs = AllGuestChairs.Count - AllCharactersInScene.FindAll(c => c.IsPriest).Count;
        
        list.Add(freeChairs <= MinFreeChairsToPlay); // número total de sillas - priests <= minimo de sillas libres
        list.Add(allGroups.All(g => g.HasPriest)); // todos los grupos tienen al menos un priest

        return list;
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
    public void ButtonComenceFeast()
    {
        currentState = UnionStates.Feasting;
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
    Feasting
}
