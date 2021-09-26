using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MainCanvas : MonoBehaviour
{
    [Header("References")]
    public UI_TimeTracker UITimeTracker;
    public GameObject ButtonComenceFeast;
    public GameObject EndGameElement;

    private List<Image> AllEndGameImages;
    private List<TextMeshProUGUI> AllEndGameTexts;

    private void Awake()
    {
        EndGameElement.SetActive(false);
        AllEndGameImages = EndGameElement.GetComponentsInChildren<Image>().ToList();
        AllEndGameTexts = EndGameElement.GetComponentsInChildren<TextMeshProUGUI>().ToList();
    }

    private void Update()
    {
        ButtonComenceFeast.SetActive(GameManager._.GetCurrentState() == UnionStates.PreparingFeast);
        UITimeTracker.gameObject.SetActive(GameManager._.GetCurrentState() == UnionStates.PreparingFeast);
        UITimeTracker.SetTimer(GameManager._.GetCurrentState() == UnionStates.PreparingFeast);
    }

    public void EndGame(float animationTime)
    {
        StartCoroutine(EndGameCR(animationTime));
    }

    private IEnumerator EndGameCR(float animationTime)
    {
        EndGameElement.SetActive(true);
        ButtonComenceFeast.SetActive(false);

        var factor = 0f;

        do
        {
            factor += Time.deltaTime / animationTime;

            foreach (var img in AllEndGameImages)
            {
                UpdateImageAlpha(img, factor);
            }
            foreach (var txt in AllEndGameTexts)
            {
                UpdateTextAlpha(txt, factor);
            }

            yield return null;
        } while(AllEndGameImages[0].color.a < 1f);
    }
    private void UpdateImageAlpha(Image img, float alpha)
    {
        var c = img.color;
        c.a = alpha;
        img.color = c;
    }
    private void UpdateTextAlpha(TextMeshProUGUI txt, float alpha)
    {
        var c = txt.color;
        c.a = alpha;
        txt.color = c;
    }
    
    // BUTTONS
    public void ButtonMainMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void ButtonExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
