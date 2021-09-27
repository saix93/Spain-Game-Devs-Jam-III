using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public UI_MainCanvas UIMainCanvas;
    public UI_TimeTracker UITimeTracker;
    public SoundManager SMG;
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
    public float TimeToStartScene = 1f;
    public MinMaxInt GuestsNumber = new MinMaxInt(4, 8);
    public bool AlwaysSpawnOddNumberOfGuests;
    public bool AlwaysSpawnMaxGuests;
    public SO_CharacterSpriteList CharacterSprites;
    public Sprite PriestSprite;
    public SO_TraitList AllTraits;
    public SO_SadnessLevelList AllSadnessLevels;
    public int TraitsPerCharacter = 3;
    public float ProcOnlyOneTrait = .1f;
    public LayerMask CharacterLayerMask;
    public LayerMask ChairLayerMask;
    public int PointsToSubstractPerRandomGroup = 1;
    public int MaxValueToAddSaddness;
    public int MinFreeChairsToPlay = 4;
    public float TimeToEndUnion = 2f;
    public float TimeBetweenUnions = 2f;
    public int NUnionsToReductTime = 5;
    public float TimeReductionEveryNUnions = 5;
    public float MinTimePerUnion = 20f;

    [Header("Animations")]
    public float TimeToMoveRandomCharacters = 1;
    public float TimeToTurnCharacterIntoPriest = 1;
    public float FadeScaleTime = 1;
    public float FadeAnimationWaitTime = 1;
    public MinMaxFloat RandomTimeBetweenReactions = new MinMaxFloat(.5f, 1.5f);
    public Sprite HeartIcon;
    public float EndGameAnimationTime = 1f;

    [Header("Realtime data")]
    public int CurrentConsecutiveUnions;
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
    private IEnumerator Start()
    {
        SMG.PlayRandomAmbientTrack();
        
        FadeCircle.SetActive(true);
        FadeMask.localScale = Vector3.zero;

        var mainMenuMusic = GameObject.FindGameObjectWithTag("MainMenuMusic");

        if (mainMenuMusic)
        {
            var audioSource = mainMenuMusic.GetComponent<AudioSource>();
            do
            {
                audioSource.volume -= Time.deltaTime / 2;
                yield return null;
            } while (audioSource.volume > 0);
        
            Destroy(mainMenuMusic);
        }
        
        yield return new WaitForSeconds(TimeToStartScene);
        var gp = new Group();
        StartUnion(gp);
    }
    private void Update()
    {
        if (currentState == UnionStates.PreparingFeast)
        {
            if (Input.GetMouseButtonDown(0)) // Left click DOWN
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
            if (Input.GetMouseButtonUp(0)) // Left click UP
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
                        if (!chair.AssignedCharacter)
                        {
                            grabbedCharacter.AssignChair(chair);
                        }
                        else
                        {
                            if (chair.AssignedCharacter.IsPriest || chair.AssignedCharacter.IsMainCharacter)
                            {
                                grabbedCharacter.transform.position = grabbedCharacterOriginalPosition;
                                return;
                            }
                            
                            // SWAP
                            if (grabbedCharacter.AssignedChair)
                            {
                                chair.AssignedCharacter.AssignChair(grabbedCharacter.AssignedChair, true);
                            }
                            else
                            {
                                chair.AssignedCharacter.AssignSpawnPoint(grabbedCharacter.AssignedSpawnPoint, true);
                            }

                            grabbedCharacter.AssignChair(chair, true);
                            return;
                        }
                    }
                    else if (spawnPoint != null)
                    {
                        if (!spawnPoint.AssignedCharacter)
                        {
                            grabbedCharacter.AssignSpawnPoint(spawnPoint);
                        }
                        else
                        {
                            if (spawnPoint.AssignedCharacter.IsPriest || spawnPoint.AssignedCharacter.IsMainCharacter)
                            {
                                grabbedCharacter.transform.position = grabbedCharacterOriginalPosition;
                                return;
                            }
                            
                            // SWAP
                            if (grabbedCharacter.AssignedChair)
                            {
                                spawnPoint.AssignedCharacter.AssignChair(grabbedCharacter.AssignedChair, true);
                            }
                            else
                            {
                                spawnPoint.AssignedCharacter.AssignSpawnPoint(grabbedCharacter.AssignedSpawnPoint, true);
                            }

                            grabbedCharacter.AssignSpawnPoint(spawnPoint, true);
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
            
            // Si se acaba el tiempo en el timer, se avanza de fase
            if (UITimeTracker.GetTimeLeft() <= 0)
            {
                if (isGrabbingCharacter)
                {
                    isGrabbingCharacter = false;
                    grabbedCharacter.IsBeingGrabbed = false;
                    grabbedCharacter.transform.position = grabbedCharacterOriginalPosition;
                }
                
                currentState = UnionStates.Feasting;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndGame();
        }
    }

    private void StartUnion(Group group)
    {
        Initialize();
        CurrentConsecutiveUnions++;
        
        currentGroup = group;

        if (currentGroup.Characters.Count < 1)
        {
            currentGroup.Characters.Add(GenerateCharacter());
            currentGroup.Characters.Add(GenerateCharacter());
        }
        
        // Setea los personajes principales para evitar que se puedan mover
        currentGroup.Characters.ForEach(c => c.IsMainCharacter = true);
        
        // Ajustes cada X bodas consecutivas
        var freeChairs = AllGuestChairs.FindAll(c => c.AssignedCharacter == null).Count;
        GuestsNumber.Max = Mathf.Min(GuestsNumber.Max, freeChairs);
        if (CurrentConsecutiveUnions % 2 == 0) // Cada 2 bodas consecutivas sube 1 el número máximo de guests a spawnear
        {
            if (GuestsNumber.Max < freeChairs) GuestsNumber.Max++;
            GuestsNumber.Min = Mathf.Min(GuestsNumber.Min + 1, GuestsNumber.Max);
        }

        if (CurrentConsecutiveUnions % NUnionsToReductTime == 0) // Cada N bodas consecutivas se baja X segundos el tiempo máximo
        {
            UITimeTracker.MaxTime = Mathf.Max(UITimeTracker.MaxTime - TimeReductionEveryNUnions, MinTimePerUnion);
        }
        
        // Reinicia el timer
        UITimeTracker.ResetTimer();

        // Genera los invitados
        var currentGuests = AllCharactersInScene.FindAll(c => !c.IsPriest && !c.IsMainCharacter).Count;
        var min = Mathf.Max(0, GuestsNumber.Min - currentGuests); // El minimo de guests se aplica sin contar los priests
        var max = GuestsNumber.Max - currentGuests;
        var guestNumber = Random.Range(min, max);
        if (AlwaysSpawnMaxGuests) guestNumber = max;
        if (AlwaysSpawnOddNumberOfGuests && (currentGuests + guestNumber) % 2 == 0)
        {
            guestNumber = Mathf.Min(guestNumber + 1, max);
            if ((currentGuests + guestNumber) % 2 == 0) guestNumber -= 1;
        }
        
        for (var i = 0; i < guestNumber; i++)
        {
            GenerateCharacter();
        }

        // Coloca a todos los personajes en una posición aleatoria
        AllCharactersInScene.ForEach(c =>
        {
            if (c.IsPriest) return;
            var spawnPos = GetRandomSpawnPosition();
            c.AssignSpawnPoint(spawnPos);
        });

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
        var priests = AllCharactersInScene.FindAll(c => c.IsPriest);
        for (var i = 0; i < priests.Count; i++)
        {
            // priests[i].AssignChair(AllGuestChairs[i]);
            var emptyChairs = AllGuestChairs.FindAll(c => c.AssignedCharacter == null);
            var randomChair = emptyChairs[Random.Range(0, emptyChairs.Count)];
            priests[i].AssignChair(randomChair);
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

        // Resta puntos a los grupos por ser generados de forma aleatoria
        var randomlyGeneratedGroups = allChairGroups.FindAll(gp => gp.RandomlyGenerated);
        randomlyGeneratedGroups.ForEach(gp => gp.Value -= PointsToSubstractPerRandomGroup);
        
        // Añade puntos de tristeza
        var sadGroups = allChairGroups.FindAll(gp => gp.Value <= MaxValueToAddSaddness);
        //sadGroups.ForEach(gp => gp.Characters.ForEach(c => c.AddSadnessPoints(1)));

        // Se muestran los emotes de los personajes
        var guests = AllCharactersInScene.FindAll(c => !c.IsMainCharacter);
        foreach (var guest in guests)
        {
            if (guest.EmoteShown) continue;
            
            var reactionTime = Random.Range(RandomTimeBetweenReactions.Min, RandomTimeBetweenReactions.Max);

            yield return new WaitForSeconds(reactionTime);

            var guestGroup = allChairGroups.Find(g => g.Characters.Contains(guest));
            guest.ShowEmote(guestGroup);

            if (sadGroups.Contains(guestGroup) && !guestGroup.SadnessAddedThisRound) // && !guestGroup.Characters.Exists(c => c != guest && c.EmoteShown)
            {
                guestGroup.Characters.ForEach(c => c.AddSadnessPoints(guestGroup.Characters.Any(ch => ch.IsPriest) ? 2 : 1));
                guestGroup.SadnessAddedThisRound = true;
                
                var extremelySadGuests = guestGroup.Characters.FindAll(c => c.SadnessLevel == SadnessLevel.Extreme && !c.IsPriest);
                foreach (var sadGuest in extremelySadGuests)
                {
                    yield return StartCoroutine(PriestAnimation(sadGuest));
                }
            }
        }
        
        yield return new WaitForSeconds(TimeToEndUnion); // Termina la fase de banquete y comienza la fase final, que termina la boda

        // Reevalua los grupos para evitar que salga ganador un grupo con priest
        allChairGroups.ForEach(g => g.ReEvaluateGroupValue());

        // Elige un grupo de entre los que más puntos tienen
        allChairGroups = allChairGroups.FindAll(gp => gp.Characters.Count > 1).OrderByDescending(x => x.Value).ToList();
        var winnerGroups = allChairGroups.FindAll(gp => gp.Value == allChairGroups[0].Value);
        var chosenGroup = winnerGroups.Count > 0 ? winnerGroups[Random.Range(0, winnerGroups.Count)] : null;
        
        // Evalúa las Lose Conditions. En caso de que alguna se cumpla, se termina la partida
        if (EvaluateLoseConditions(allChairGroups, chosenGroup).Any(b => b))
        {
            EndGame();
            yield break;
        }
        
        // Reproduce un nuevo track
        SMG.PlayRandomAmbientTrack();
        
        // Muestra el emote del corazón en la nueva pareja
        chosenGroup.Characters.ForEach(c => c.ShowSpecificEmote(HeartIcon));
        yield return StartCoroutine(FadeAnimation(chosenGroup.GetMiddlePosition(), -1));

        currentGroup.Characters.ForEach(c => DestroyImmediate(c.gameObject));
        AllCharactersInScene.RemoveAll(item => item == null);
        
        // Esperamos a empezar la siguiente boda
        yield return new WaitForSeconds(TimeBetweenUnions);

        StartUnion(chosenGroup);
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

    private List<bool> EvaluateLoseConditions(List<Group> allGroups, Group chosenGroup)
    {
        var list = new List<bool>();
        var freeChairs = AllGuestChairs.Count - AllCharactersInScene.FindAll(c => c.IsPriest).Count;
        
        list.Add(freeChairs <= MinFreeChairsToPlay); // número total de sillas - priests <= minimo de sillas libres
        list.Add(allGroups.All(g => g.HasPriest)); // todos los grupos tienen al menos un priest
        list.Add(allGroups.FindAll(g => !g.HasPriest).All(g => g.Characters.Count < 2)); // no hay ningún grupo con más de 1 integrante (sin contar priests)
        if (chosenGroup != null) list.Add(chosenGroup.Characters.Count < 2); // El grupo elegido tiene menos de 2 personajes

        return list;
    }
    private void EndGame()
    {
        StopAllCoroutines();
        UIMainCanvas.EndGame(EndGameAnimationTime);
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
        var rng = Random.value;
        if (rng < ProcOnlyOneTrait)
        {
            num = 1;
        }

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
    public void ButtonFullReset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
