using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Item.Common;
using _1.Scripts.Manager.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using UIManager = _1.Scripts.Manager.Subs.UIManager;

namespace _1.Scripts.UI.InGame.HUD
{
    public class InGameUI : UIBase
    {
        [Header("플레이어 상태")] 
        [SerializeField] private Slider staminaSlider;
        [SerializeField] private Slider armorSlider;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI maxHealthText;

        [Header("체력바")] 
        [SerializeField] private GameObject healthSegmentPrefab;
        [SerializeField] private Transform healthSegmentContainer;
        [SerializeField] private int healthSegmentValue = 10;
        [SerializeField] private Animator healthBackgroundAnimator;
        private List<Animator> healthSegmentAnimators = new List<Animator>();
        private List<Image> healthSegments = new List<Image>();
        private float prevHealth;

        [Header("스테미나")] 
        [SerializeField] private Animator staminaAnimator;
        private float lackValue = 0.2f;
        
        [Header("게이지")] 
        [SerializeField] private Image focusGaugeImage;
        [SerializeField] private Image instinctGaugeImage;
        [SerializeField] private Animator instinctEffectAnimator;
        [SerializeField] private Animator focusEffectAnimator;
        private Coroutine focusEffectCoroutine;
        private Coroutine instinctEffectCoroutine;
        
        [Header("ItemUseUI")]
        [SerializeField] private Image progressFillImage;
        [SerializeField] private TextMeshProUGUI toastText;
        
        public CrosshairController CrosshairController => crosshairController;
        [SerializeField] private CrosshairController crosshairController;    
        
        private PlayerCondition playerCondition;
        private bool isPaused = false;
        
        private void Awake() { progressFillImage.enabled = false; }
        
        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            gameObject.SetActive(false);
        }

        public override void Show()
        {
            base.Show();
            if (!playerCondition) return;
            if (playerCondition.CurrentFocusGauge >= 1f)
                focusEffectCoroutine ??= StartCoroutine(FocusEffectCoroutine());
            if (playerCondition.CurrentInstinctGauge >= 1f)
                instinctEffectCoroutine ??= StartCoroutine(InstinctEffectCoroutine());
        }

        public override void Hide()
        {
            if (focusEffectCoroutine != null) { StopCoroutine(focusEffectCoroutine); focusEffectCoroutine = null; }
            if (instinctEffectCoroutine != null) { StopCoroutine(instinctEffectCoroutine); instinctEffectCoroutine = null; }
            base.Hide();
        }

        public override void ResetUI()
        {
            ResetStatement();
        }
        
        public void UpdateStateUI()
        {
            playerCondition = CoreManager.Instance.gameManager.Player.PlayerCondition;
            
            Initialize_HealthSegments();
            UpdateHealthSlider(playerCondition.CurrentHealth, playerCondition.MaxHealth);
            UpdateStaminaSlider(playerCondition.CurrentStamina, playerCondition.MaxStamina);
            UpdateArmorSlider(playerCondition.CurrentShield, playerCondition.MaxShield);
            UpdateInstinct(playerCondition.CurrentInstinctGauge);
            UpdateFocus(playerCondition.CurrentFocusGauge);
        }
        
        private void Initialize_HealthSegments()
        {
            if (healthSegments.Count > 0)
            {
                prevHealth = playerCondition.CurrentHealth;
                return;
            }

            if (!healthSegmentPrefab || !healthSegmentContainer) return;
            int count = playerCondition.MaxHealth / healthSegmentValue;
            for (int i = 0; i < count; i++)
            {
                var segment = Instantiate(healthSegmentPrefab, healthSegmentContainer, false);
                if(!segment.TryGetComponent(out Image img)) { Destroy(segment); continue; }
                img.type = Image.Type.Filled;
                img.fillAmount = 1f;
                healthSegments.Add(img);
                segment.gameObject.SetActive(true);
                if (segment.TryGetComponent(out Animator animator)) healthSegmentAnimators.Add(animator);
            }
        }
        
        public void UpdateHealthSlider(float current, float max)
        {
            healthText.text = $"{current}";
            maxHealthText.text = $"{max}";
            int full = Mathf.FloorToInt(current / healthSegmentValue);
            float partial = (current % healthSegmentValue) / healthSegmentValue;
            for (int i = 0; i < healthSegments.Count; i++)
            {
                if (i < full) healthSegments[i].fillAmount = 1f;
                else if (i == full) healthSegments[i].fillAmount = partial;
                else healthSegments[i].fillAmount = 0f;
            }

            if (healthBackgroundAnimator && current < prevHealth) healthBackgroundAnimator.SetTrigger("Damaged");
            if (current < prevHealth && healthSegmentAnimators != null)
            {
                for (int i = 0; i < full && i < healthSegmentAnimators.Count; i++)
                    healthSegmentAnimators[i].SetTrigger("Damaged");
            }
            prevHealth = current;
        }

        public void UpdateStaminaSlider(float current, float max)
        {
            float ratio = current / max; 
            staminaSlider.value = ratio;
            staminaAnimator.SetBool("IsLack", ratio < lackValue);
        }

        public void UpdateArmorSlider(float current, float max)
        {
            if (armorSlider && max > 0)
            {
                armorSlider.enabled = true;
                armorSlider.value = current / max;
            }
            else if (armorSlider || max == 0 || current == 0)
                armorSlider.enabled = false;
        }

        public void UpdateInstinct(float value)
        {
            float instinct = Mathf.Clamp01(value);
            instinctGaugeImage.fillAmount = instinct;
            
            if (!gameObject.activeInHierarchy) return;
            
            bool canTriggerInstinctEffect = instinct >= 1f && playerCondition && playerCondition.CurrentHealth <= 50;
            
            if (canTriggerInstinctEffect && instinctEffectCoroutine == null) instinctEffectCoroutine = StartCoroutine(InstinctEffectCoroutine());
            else if (!canTriggerInstinctEffect && instinctEffectCoroutine != null)
            {
                StopCoroutine(instinctEffectCoroutine);
                instinctEffectCoroutine = null;
            }
        }

        public void UpdateFocus(float value)
        {
            float focus = Mathf.Clamp01(value);
            focusGaugeImage.fillAmount = focus;
            
            if (!gameObject.activeInHierarchy) return;
            if (focus >= 1f && focusEffectCoroutine == null) focusEffectCoroutine = StartCoroutine(FocusEffectCoroutine());
            else if (focus < 1f && focusEffectCoroutine != null)
            {
                StopCoroutine(focusEffectCoroutine);
                focusEffectCoroutine = null;
            }
        }
        
        private IEnumerator FocusEffectCoroutine()
        {
            while (true)
            {
                focusEffectAnimator.ResetTrigger("Full");
                focusEffectAnimator.SetTrigger("Full");
                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator InstinctEffectCoroutine()
        {
            while (true)
            {
                instinctEffectAnimator.ResetTrigger("Full");
                instinctEffectAnimator.SetTrigger("Full");
                yield return new WaitForSeconds(0.5f);
            }
        }

        public void ShowItemProgress() { progressFillImage.enabled = true; }

        public void HideItemProgress() { progressFillImage.enabled = false; }

        public void UpdateItemProgress(float progress) { progressFillImage.fillAmount = Mathf.Clamp01(progress); }

        
        public void ShowToast(string key)
        {
            var localized = new LocalizedString("New Table", key);
            StartCoroutine(SetToastAndHide(localized));
        }
        public void ShowToast(ItemData itemData)
        {
            var localizedDesc = new LocalizedString("New Table", itemData.DescriptionKey);
            StartCoroutine(SetToastAndHide(localizedDesc));
        }
        private IEnumerator SetToastAndHide(LocalizedString localized)
        {
            var handle = localized.GetLocalizedStringAsync();
            yield return handle;
            toastText.text = handle.Result;
            toastText.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            toastText.gameObject.SetActive(false);
        }

        private void ResetStatement()
        {
            healthSegmentAnimators.Clear();
            foreach(var segment in healthSegments) Destroy(segment.gameObject);
            healthSegments.Clear();
            
            healthText.text = ""; maxHealthText.text = "";
            staminaSlider.value = 0f; staminaAnimator.SetBool("IsLack", false);
            armorSlider.value = 0f; armorSlider.enabled = false;
            instinctGaugeImage.fillAmount = 0f;
            focusGaugeImage.fillAmount = 0f;
            if (instinctEffectCoroutine != null)
            {
                StopCoroutine(instinctEffectCoroutine);
                instinctEffectCoroutine = null;
            }
            if (focusEffectCoroutine != null)
            {
                StopCoroutine(focusEffectCoroutine);
                focusEffectCoroutine = null;
            }
            progressFillImage.enabled = false;
            progressFillImage.fillAmount = 0f;
        }
    }
}