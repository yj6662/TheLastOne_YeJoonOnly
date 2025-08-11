using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Util;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using _1.Scripts.Weapon.Scripts.Hack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _1.Scripts.UI.InGame.HUD
{
    public class WeaponUI : UIBase
    {
        [Header("Slot")]
        [SerializeField] private List<RectTransform> slotTransforms;
        [SerializeField] private List<Animator> slotAnimators;
 
        [Header("WeaponInfo")]
        [SerializeField] private List<Image> slotImages; 
        [SerializeField] private List<TextMeshProUGUI> slotTexts;
        [SerializeField] private List<TextMeshProUGUI> slotAmmoTexts;
        [SerializeField] private TextMeshProUGUI currentAmmoText;
        [SerializeField] private TextMeshProUGUI currentTotalAmmoText;
        [SerializeField] private Image ammoSlotFrame;
        [SerializeField] private RectTransform currentAmmoRectTransform;
        
        [SerializeField] private float shakeDuration = 0.2f;
        [SerializeField] private float shakeStrength = 3f;

        [Header("Scale 세팅")]
        [SerializeField] private Vector3 normalScale = Vector3.one;
        [SerializeField] private Vector3 selectedScale = new Vector3(1.5f,1.2f,1f);
        [SerializeField] private float scaleSpeed = 10f;
        
        [Header("컬러 세팅")]
        [SerializeField] private Color selectedAmmoColor = Color.white;
        [SerializeField] private Color selectedColor = Color.black;
        [SerializeField] private List<Image> selectedSlotImages;
        [SerializeField] private float idleAlpha = 0.5f;
        [SerializeField] private float selectedSlotAlpha = 1f;
        [SerializeField] private Color emptyAmmoColor = Color.red;
        
        [Header("애니메이터")] 
        [SerializeField] private Animator panelAnimator;
        [SerializeField] private float panelHideDelay = 3f;

        private static readonly WeaponType[] SlotOrder = new[]
        {
            WeaponType.Rifle,
            WeaponType.Pistol,
            WeaponType.GrenadeLauncher,
            WeaponType.HackGun
        };
        
        private PlayerCondition playerCondition;
        private PlayerWeapon playerWeapon;
        private Dictionary<WeaponType, BaseWeapon> ownedWeapons = new();
        
        private Vector3[] targetScales;
        private Vector3 originalLocalPosition;
        
        private Coroutine emptyFlashCoroutine;
        private Color originalAmmoColor;
        private Coroutine hideCoroutine;
        private Coroutine shakeCoroutine;
        private int lastMag = -1;
        private bool isPanelVisible = false;
        
        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            targetScales = new Vector3[slotTransforms.Count];
            
            if (currentAmmoRectTransform) originalLocalPosition = currentAmmoRectTransform.localPosition;
            if (currentAmmoText) originalAmmoColor = currentAmmoText.color;
            gameObject.SetActive(false);
        }
        
        public override void ResetUI()
        {
            for (int i = 0; i < slotTransforms.Count; i++)
            {
                slotTransforms[i].localScale = normalScale;
                slotImages[i].color = Color.clear;
                slotTexts[i].text = string.Empty;
                slotAmmoTexts[i].text = string.Empty;
                slotAmmoTexts[i].enabled = false;
                slotTexts[i].enabled = false;
                SetSlotAlpha(i, idleAlpha);
                if (targetScales != null && i < targetScales.Length) targetScales[i] = normalScale;
            }
            foreach (var image in selectedSlotImages)
            {
                if (!image) continue;
                var color = image.color;
                color.a = idleAlpha;
                image.color = color;
            }
            currentAmmoText.text = string.Empty;
            currentTotalAmmoText.text = string.Empty;
            currentAmmoText.color = originalAmmoColor;
            if (emptyFlashCoroutine != null) { StopCoroutine(emptyFlashCoroutine); emptyFlashCoroutine = null; }
            if (ammoSlotFrame) ammoSlotFrame.gameObject.SetActive(false);
            if (hideCoroutine != null) { StopCoroutine(hideCoroutine); hideCoroutine = null; }
            if (!panelAnimator) return;
            panelAnimator.ResetTrigger("Show");
            panelAnimator.ResetTrigger("Hide");
        }

        private void Update()
        {
            if (targetScales == null) return;
            for (int i = 0; i < slotTransforms.Count; i++) 
                slotTransforms[i].localScale = Vector3.Lerp(slotTransforms[i].localScale, targetScales[i], Time.deltaTime * scaleSpeed);
        }
        private WeaponType GetSlotWeaponType(int slotIdx)
        {
            var role = SlotOrder[slotIdx];
            if (role != WeaponType.Pistol) return role;
            return ownedWeapons.ContainsKey(WeaponType.SniperRifle) ? WeaponType.SniperRifle : WeaponType.Pistol;
        }
        public void Refresh(bool playShowAnimation = true)
        {
            playerCondition = CoreManager.Instance.gameManager.Player.PlayerCondition;
            playerWeapon = CoreManager.Instance.gameManager.Player.PlayerWeapon;
            
            ownedWeapons.Clear();
            if (playerWeapon)
            {
                foreach (var kv in playerWeapon.Weapons)
                {
                    var type = kv.Key;
                    var weapon = kv.Value;
                    if (!weapon) continue;
                    if (!playerWeapon.AvailableWeapons.TryGetValue(type, out var unlocked) || !unlocked) continue;
                    if (type == WeaponType.Punch) continue;
                    ownedWeapons[type] = weapon;
                }
            }

            WeaponType equippedType = playerCondition ? playerCondition.EquippedWeaponIndex : WeaponType.Rifle;
            int equippedSlotIdx = -1;
            for (int i = 0; i < SlotOrder.Length; i++)
            {
                if (GetSlotWeaponType(i) != equippedType) continue;
                equippedSlotIdx = i;
                break;
            }
            bool isPunchEquipped = (equippedType == WeaponType.Punch);

            for (int i = 0; i < slotTransforms.Count && i < SlotOrder.Length; i++)
            {
                WeaponType type = GetSlotWeaponType(i);
                bool slotHasWeapon = ownedWeapons.TryGetValue(type, out var weapon);

                if (slotHasWeapon)
                {
                    slotImages[i].color = Color.white;
                    slotImages[i].sprite = SlotUtility.GetWeaponSprite(weapon);
                    SlotUtility.GetWeaponName(weapon, slotTexts[i]);
                    var (mag, total) = SlotUtility.GetWeaponAmmo(weapon);
                    slotAmmoTexts[i].text = (mag > 0 || total > 0) ? $"{mag}/{total}" : "";
                    slotAmmoTexts[i].color = weapon is Gun ? selectedColor : selectedAmmoColor;
                }
                else
                {
                    slotImages[i].color = Color.clear;
                    slotImages[i].sprite = null;
                    slotTexts[i].text = "";
                    slotAmmoTexts[i].text = "";
                }
                
                bool isSelected = (!isPunchEquipped && i == equippedSlotIdx && slotHasWeapon);

                slotTexts[i].enabled = isSelected;
                slotAmmoTexts[i].enabled = isSelected && slotAmmoTexts[i].text != "";
                SetSlotAlpha(i, isSelected ? selectedSlotAlpha : idleAlpha);
                targetScales[i] = isSelected ? selectedScale : normalScale;
            }
            if (isPunchEquipped)
            {
                if (ammoSlotFrame) ammoSlotFrame.gameObject.SetActive(false);
                currentAmmoText.text = "";
                currentTotalAmmoText.text = "";
            }
            else
            {
                UpdateCurrentAmmoText(equippedSlotIdx);
            }

            if (isPunchEquipped || !playShowAnimation || equippedSlotIdx < 0 ||
                equippedSlotIdx >= slotAnimators.Count) return;
            slotAnimators[equippedSlotIdx]?.Rebind();
            slotAnimators[equippedSlotIdx]?.Play(0);
            panelAnimator?.ResetTrigger("Show");
            panelAnimator?.ResetTrigger("Hide");
            panelAnimator?.SetTrigger("Show");
            if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(HidePanelCoroutine());
        }    
        public void PlayEmptyFlash(float duration = 0.5f)
        {
            if (emptyFlashCoroutine != null) StopCoroutine(emptyFlashCoroutine);
            emptyFlashCoroutine = StartCoroutine(EmptyAmmoCoroutine(duration));
        }

        private IEnumerator HidePanelCoroutine()
        {
            yield return new WaitForSeconds(panelHideDelay);
            panelAnimator?.ResetTrigger("Show");
            panelAnimator?.ResetTrigger("Hide");
            panelAnimator?.SetTrigger("Hide");
            isPanelVisible = false;
            hideCoroutine = null;
        }
        
        private void SetSlotAlpha(int index, float alpha)
        {
            if (!selectedSlotImages[index]) return;
            var color = selectedSlotImages[index].color;
            color.a = alpha;
            selectedSlotImages[index].color = color;
        }

        private void UpdateCurrentAmmoText(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= SlotOrder.Length)
            {
                ammoSlotFrame.gameObject.SetActive(false);
                currentAmmoText.text = string.Empty;
                currentTotalAmmoText.text = string.Empty;
                return;
            }
            WeaponType type = GetSlotWeaponType(selectedIndex);

            if (!ownedWeapons.TryGetValue(type, out var currentWeapon) || type == WeaponType.Punch)
            {
                ammoSlotFrame.gameObject.SetActive(false);
                currentAmmoText.text = string.Empty;
                currentTotalAmmoText.text = string.Empty;
                return;
            }

            var (mag, total) = SlotUtility.GetWeaponAmmo(currentWeapon);
            if (lastMag != -1 && mag < lastMag)
            {
                if (shakeCoroutine != null)
                    StopCoroutine(shakeCoroutine);
                shakeCoroutine = StartCoroutine(ShakeCoroutine());
            }
            lastMag = mag;
            if (mag > 0 || total > 0)
            {
                ammoSlotFrame.gameObject.SetActive(true);
                currentAmmoText.text = $"{mag}";
                currentTotalAmmoText.text = $"{total}";
            }
            else
            {
                ammoSlotFrame.gameObject.SetActive(false);
                currentAmmoText.text = string.Empty;
                currentTotalAmmoText.text = string.Empty;
            }
        }

        private IEnumerator ShakeCoroutine()
        {
            float timer = 0f;
            while (timer < shakeDuration)
            {
                float offsetX = Random.Range(-shakeStrength, shakeStrength);
                float offsetY = Random.Range(-shakeStrength, shakeStrength);
                currentAmmoRectTransform.localPosition = originalLocalPosition + new Vector3(offsetX, offsetY, 0f);
                timer += Time.deltaTime;
                yield return null;
            }
            currentAmmoRectTransform.localPosition = originalLocalPosition;
        }
        
        private IEnumerator EmptyAmmoCoroutine(float duration)
        {
            float t = 0f;
            float flashSpeed = 6f;
            float lerpSpeed = 2f;
            bool isRed = true;

            while (t < duration)
            {
                currentAmmoText.color = isRed ? emptyAmmoColor : originalAmmoColor;
                isRed = !isRed;
                yield return new WaitForSecondsRealtime(1f / flashSpeed);
                t += 1f / flashSpeed;
            }

            t = 0f;
            Color startColor = currentAmmoText.color;
            while (t < 1f)
            {
                currentAmmoText.color = Color.Lerp(startColor, originalAmmoColor, t);
                t += Time.unscaledDeltaTime * lerpSpeed;
                yield return null;
            }
            currentAmmoText.color = originalAmmoColor;
            emptyFlashCoroutine = null;
        }
    }
}
