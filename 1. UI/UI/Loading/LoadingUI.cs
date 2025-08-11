using System.Collections;
using _1.Scripts.Manager.Subs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.Loading
{
    public class LoadingUI : UIBase
    {
        [Header("Loading UI")]
        [SerializeField] private GameObject panel;
        public Slider progressSlider;
        public TextMeshProUGUI progressText;

        [SerializeField] private GameObject tutorialPanel1;
        [SerializeField] private GameObject tutorialPanel2;
        [SerializeField] private CanvasGroup tutorialCanvasGroup1;
        [SerializeField] private CanvasGroup tutorialCanvasGroup2;
        
        private Coroutine fadeCoroutine;
        private float fadeDuration = 0.5f;
        private bool isTutorial1Shown = true;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            Hide();
        }

        public override void Show()
        {
            panel.SetActive(true);
            tutorialCanvasGroup1.gameObject.SetActive(true);
            tutorialCanvasGroup1.alpha = 1;
            tutorialCanvasGroup2.gameObject.SetActive(false);
            tutorialCanvasGroup2.alpha = 0;
            isTutorial1Shown = true;
        }

        public override void Hide()
        {
            panel.SetActive(false);
        }

        public override void ResetUI()
        {
            progressSlider.value = 0f;
            progressText.text = "0.00%";
            ShowTutorialByProgress(0f);
        }
        
        public void UpdateLoadingProgress(float progress)
        {
            progressSlider.value = progress;
            progressText.text = $"{progress * 100:0.00}%";
            ShowTutorialByProgress(progress);
        }

        private void ShowTutorialByProgress(float progress)
        {
            switch (progress)
            {
                case < 0.5f when !isTutorial1Shown:
                    SwitchPanel(tutorialCanvasGroup1, tutorialCanvasGroup2);
                    isTutorial1Shown = true;
                    break;
                case >= 0.5f when isTutorial1Shown:
                    SwitchPanel(tutorialCanvasGroup2, tutorialCanvasGroup1);
                    isTutorial1Shown = false;
                    break;
            }
        }
        private void SwitchPanel(CanvasGroup fadeIn, CanvasGroup fadeOut)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadePanel(fadeIn, fadeOut));
        }

        public void UpdateProgressText(string text)
        {
            progressText.text = text;
        }

        private IEnumerator FadePanel(CanvasGroup fadeIn, CanvasGroup fadeOut)
        {
            fadeIn.gameObject.SetActive(true);
            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float normalized = t / fadeDuration;
                fadeIn.alpha = Mathf.Lerp(0, 1, normalized);
                if (fadeOut) fadeOut.alpha = Mathf.Lerp(1, 0, normalized);
                yield return null;
            }
            fadeIn.alpha = 1;
            if (!fadeOut) yield break;
            fadeOut.alpha = 0;
            fadeOut.gameObject.SetActive(false);
        }
    }
}