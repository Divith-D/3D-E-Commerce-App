using System.Collections;
using TMPro;
using UnityEngine;

public class EmptyStateView : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField]
    private CanvasGroup canvasGroup;


    [Header("Optional Text")]
    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private TextMeshProUGUI messageText;


    [Header("Animation")]
    [SerializeField]
    private float fadeDuration = 0.20f;


    private Coroutine fadeRoutine;


    private void Awake()
    {
        HideImmediate();


        if (titleText != null)
        {
            titleText.text =
                "No products found";
        }


        if (messageText != null)
        {
            messageText.text =
                "Try changing or clearing your filters.";
        }
    }


    public void Show()
    {
        SetVisible(true);
    }


    public void Hide()
    {
        SetVisible(false);
    }


    public void SetVisible(
        bool visible)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(
                fadeRoutine
            );
        }


        fadeRoutine =
            StartCoroutine(
                FadeCanvasGroup(
                    visible
                )
            );
    }


    private IEnumerator FadeCanvasGroup(
        bool visible)
    {
        if (canvasGroup == null)
            yield break;


        float startAlpha =
            canvasGroup.alpha;

        float targetAlpha =
            visible ? 1f : 0f;

        float elapsed = 0f;


        if (visible)
        {
            canvasGroup
                .blocksRaycasts = true;
        }
        else
        {
            canvasGroup
                .interactable = false;

            canvasGroup
                .blocksRaycasts = false;
        }


        while (
            elapsed <
            fadeDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    fadeDuration
                );


            t =
                t * t *
                (3f - (2f * t));


            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    t
                );


            yield return null;
        }


        canvasGroup.alpha =
            targetAlpha;

        canvasGroup.interactable =
            visible;

        canvasGroup.blocksRaycasts =
            visible;


        fadeRoutine = null;
    }


    public void HideImmediate()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(
                fadeRoutine
            );

            fadeRoutine = null;
        }


        if (canvasGroup == null)
            return;


        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}