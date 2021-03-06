using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Character : MonoBehaviour
{
    [Header("References")]
    public UI_Trait UITraitPrefab;
    public GameObject TraitsElement;
    public Transform TraitContainer;
    public GameObject SadnessLevelContainer;
    public List<GameObject> SadnessLevelBars;
    public Image Bubble;
    public Image BubbleIcon;

    [Header("Data")]
    public float EmotionLastingTime = 2f;
    public float BubbleAnimationTime = .5f;

    private Canvas canvas;
    private Character character;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    private void Update()
    {
        if (!character) return;
        
        var conditions = !character.IsMainCharacter && !character.IsPriest;
        TraitsElement.SetActive(conditions);
        SadnessLevelContainer.SetActive(conditions);
        UpdateSadnessInfo(conditions);
    }

    public void Init(Character newCharacter)
    {
        character = newCharacter;
        
        Utils.DestroyChildren(TraitContainer);
        character.Traits.ForEach(t =>
        {
            var i = Instantiate(UITraitPrefab, TraitContainer);
            i.Init(t);
        });
        
        UpdateAlpha(Bubble, 0);
        UpdateAlpha(BubbleIcon, 0);
    }

    private void UpdateSadnessInfo(bool conditions)
    {
        if (!conditions) return;
        
        var level = Mathf.Min(character.SadnessPoints, SadnessLevelBars.Count - 1);

        for (var i = 0; i < SadnessLevelBars.Count; i++)
        {
            var bar = SadnessLevelBars[i];
            bar.SetActive(i == level);
        }
    }

    public void ChangeOrderInLayer(int newOrder)
    {
        canvas.sortingOrder = newOrder;
    }

    public void ShowEmote(Sprite icon)
    {
        StartCoroutine(DisplayEmotion(icon));
    }

    private IEnumerator DisplayEmotion(Sprite icon)
    {
        var factor = 0f;
        BubbleIcon.sprite = icon;
        
        do
        {
            factor += Time.deltaTime / BubbleAnimationTime;
            
            UpdateAlpha(Bubble, factor);
            UpdateAlpha(BubbleIcon, factor);
            
            yield return null;
        } while (Bubble.color.a < 1f);
        
        yield return new WaitForSeconds(EmotionLastingTime);

        // factor = 1f;
        do
        {
            factor -= Time.deltaTime / BubbleAnimationTime;
            
            UpdateAlpha(Bubble, factor);
            UpdateAlpha(BubbleIcon, factor);
            
            yield return null;
        } while (Bubble.color.a > 0f);
    }

    private void UpdateAlpha(Image img, float alpha)
    {
        var c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
