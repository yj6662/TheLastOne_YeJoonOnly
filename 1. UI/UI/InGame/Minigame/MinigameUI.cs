using System.Collections;
using _1.Scripts.Manager.Subs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame.Minigame
{
    public class MinigameUI : UIBase
    {
        [Header("Main UI Components")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Animator animator;
        [SerializeField] private TextMeshProUGUI clearText;
        [SerializeField] private TextMeshProUGUI loopText;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private GameObject enterText;
        [SerializeField] private Slider timeSlider;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Transform contentRoot;
        
        [Header("Minigame UI Components")]
        [SerializeField] private AlphabetMatchingUI alphabetMatchingUI;
        [SerializeField] private WireConnectionUI wireConnectionUI;
        [SerializeField] private ChargeBarUI chargeBarUI;
        
        public AlphabetMatchingUI GetAlphabetMatchingUI() => alphabetMatchingUI;
        public WireConnectionUI GetWireConnectionUI() => wireConnectionUI;
        public ChargeBarUI GetChargeBarUI() => chargeBarUI;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            panel.SetActive(false);
        }
        
        public override void ResetUI()
        {
            ShowPanel(false);
            ShowCountdownText(false);
            ShowClearText(false);
            ShowEnterText(false);
            SetTimeSlider(0f, 0f);
            ShowTimeSlider(false);
            ShowLoopText(false);

            ClearTexts();
            
            alphabetMatchingUI.Hide();
            wireConnectionUI.Hide();
            chargeBarUI.Hide();
        }

        public override void Hide()
        {
            if (isActiveAndEnabled) StartCoroutine(HidePanelCoroutine());
            else panel.SetActive(false);
        }

        public void HideImmediately() { panel.SetActive(false); }

        private IEnumerator HidePanelCoroutine()
        {
            if (!gameObject.activeInHierarchy)
            {
                panel.SetActive(false);
                yield break;
            }
            animator.Play("Window Out");
            ShowClearText(false);
            yield return new WaitForSeconds(0.5f);
            panel.SetActive(false);
        }
        
        public void ShowPanel(bool show = true)
        {
            panel.SetActive(show);
            if (show) animator.Play("Window In");
        }
        public void StartCountdownUI(float time)
        {
            ShowCountdownText(true);
            ShowEnterText(false);
            ShowClearText(false);
            ShowLoopText(false);
            ShowTimeSlider(false);
        }
        public void StartTimerUI(float duration)
        {
            ShowCountdownText(false);
            SetTimeSlider(duration, duration);
            ShowTimeSlider(true);
        }
        public void ShowEndResult(bool success, string resultText = null)
        {
            ShowClearText(true);
            SetClearText(success, string.IsNullOrEmpty(resultText) ? (success ? "CLEAR!" : "FAIL") : resultText);
            SetTimeSlider(0, 0);
            ShowTimeSlider(false);
            ShowLoopText(false);
            ShowDescriptionText(false);
        }

        public void SetDescriptionText(string text) { descriptionText.text = text; }
        private void ShowDescriptionText(bool show = true) => descriptionText.gameObject.SetActive(show);
        public void ShowLoopText(bool show = true) => loopText.gameObject.SetActive(show);
        private void ShowClearText(bool show = true) => clearText.gameObject.SetActive(show);

        private void SetClearText(bool success, string text)
        {
            clearText.text = text;
            clearText.color = success ? Color.cyan : Color.red;
        }

        private void ShowEnterText(bool show = true) => enterText.SetActive(show);
        public void ShowCountdownText(bool show = true) => countdownText.gameObject.SetActive(show);
        public void SetCountdownText(float t) => countdownText.text = t > 0 ? t.ToString("F0") : "0";
        private void ShowTimeSlider(bool show = true) => timeSlider.gameObject.SetActive(show);

        private void SetTimeSlider(float current, float max)
        {
            timeSlider.maxValue = max;
            timeSlider.value = current;
            timeText.text = $"{current:0.00}s";
        }
        public void UpdateTimeSlider(float current)
        {
            timeSlider.value = current;
            timeText.text = $"{current:0.00}s";
        }
        public void UpdateLoopCount(int current, int max) => loopText.text = $"{current}/{max}";

        public void ShowAlphabetMatching(bool show = true)
        {
            alphabetMatchingUI.gameObject.SetActive(show);
            if (!show) alphabetMatchingUI.ResetUI();
        }
        
        public void SetMiniGame(string description = "", bool showLoop = false)
        {
            ShowPanel(true);
            SetDescriptionText(description);
            ShowDescriptionText(true);
            ShowLoopText(showLoop);
            ShowCountdownText(false);
            ShowClearText(false);
            ShowEnterText(true);
            SetTimeSlider(0f, 0f);
            ShowTimeSlider(false);
        }

        private void ClearTexts()
        {
            clearText.text = "";
            loopText.text = "";
            countdownText.text = "";
            descriptionText.text = "";
            timeText.text = "";
            timeSlider.value = 0;
        }
    }
}