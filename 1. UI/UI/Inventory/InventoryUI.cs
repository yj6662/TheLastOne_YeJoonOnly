using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Util;
using _1.Scripts.Weapon.Scripts.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.Inventory
{
    public class InventoryUI : UIBase
    {
        private static readonly WeaponType[] SlotOrder = new[]
        {
            WeaponType.Rifle,
            WeaponType.Pistol,
            WeaponType.GrenadeLauncher,
            WeaponType.HackGun
        };
        
        [Header("UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Animator panelAnimator;
        
        [Header("Preview Image")]
        [SerializeField] private List<Button> weaponButtons;
        
        [Header( "Preview")]
        [SerializeField] private Transform previewSpawnPoint;
        private Dictionary<WeaponType, GameObject> weaponPreviewMap;

        [Header("StatsUI")]  
        [SerializeField] private Slider damageSlider;
        [SerializeField] private Slider rpmSlider;
        [SerializeField] private Slider recoilSlider;
        [SerializeField] private Slider weightSlider;
        [SerializeField] private Slider ammoSlider;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI rpmText;
        [SerializeField] private TextMeshProUGUI recoilText;
        [SerializeField] private TextMeshProUGUI weightText;
        [SerializeField] private TextMeshProUGUI ammoText;

        [Header("Description")] 
        [SerializeField] private List<Image> weaponImages;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        
        private PlayerCondition playerCondition;
        private PlayerWeapon playerWeapon;

        private Dictionary<WeaponType, BaseWeapon> ownedWeapons = new();
        private WeaponType? currentWeaponType = null;
        
        private int maxDamage, maxAmmo;
        private float maxRPM, maxRecoil, maxWeight;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            weaponPreviewMap = new();
            foreach (Transform child in previewSpawnPoint)
            {
                var handler = child.GetComponent<PreviewWeaponHandler>();
                if (handler) weaponPreviewMap[handler.weaponType] = child.gameObject;
                child.gameObject.SetActive(false);
            }

            playerCondition = CoreManager.Instance.gameManager.Player.PlayerCondition;
            playerWeapon = CoreManager.Instance.gameManager.Player.PlayerWeapon;
            SyncOwnedWeapons();
            gameObject.SetActive(false);
        }

        public override void Show()
        {
            base.Show();
            panelAnimator?.Rebind();
            panelAnimator?.Play("Panel In");
            
            RefreshInventoryUI();

            playerCondition.OnDisablePlayerMovement();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        public override void Hide() 
        {
            if (panelAnimator && panelAnimator.isActiveAndEnabled)
            {
                panelAnimator.Rebind();
                panelAnimator.Play("Panel Out");
                StartCoroutine(HideCoroutine());
            }
            else base.Hide();
            
            playerCondition.OnEnablePlayerMovement();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void ResetUI()
        {
            foreach (var btn in weaponButtons)
            {
                btn.gameObject.SetActive(false);
                btn.onClick.RemoveAllListeners();
                var label = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (label) label.text = "";
            }
            foreach (var go in weaponPreviewMap.Values) go.SetActive(false);
            titleText.text = descriptionText.text = damageText.text = rpmText.text = recoilText.text = weightText.text = ammoText.text = "";
            damageSlider.value = rpmSlider.value = recoilSlider.value = weightSlider.value = ammoSlider.value = 0f;
            currentWeaponType = null;
        }
        
        private WeaponType GetSlotWeaponType(int slotIdx)
        {
            var role = SlotOrder[slotIdx];
            if (role != WeaponType.Pistol) return role;
            return ownedWeapons.ContainsKey(WeaponType.SniperRifle) ? WeaponType.SniperRifle : WeaponType.Pistol;
        }

        private void SyncOwnedWeapons()
        {
            ownedWeapons.Clear();
            if (!playerWeapon) return;
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

        private void CalculateMaxStats()
        {
            maxDamage = 0; maxRPM = 0f; maxRecoil = 0f; maxWeight = 0f; maxAmmo = 0;
            if (ownedWeapons.Count == 0) return;
            foreach (var stat in ownedWeapons.Values.Select(SlotUtility.GetWeaponStat))
            {
                maxDamage = Mathf.Max(maxDamage, stat.Damage);
                maxRPM = Mathf.Max(maxRPM, stat.Rpm);
                maxRecoil = Mathf.Max(maxRecoil, stat.Recoil);
                maxWeight = Mathf.Max(maxWeight, stat.Weight);
                maxAmmo = Mathf.Max(maxAmmo, stat.MaxAmmoCountInMagazine);
            }
        }

        private void InitializeWeaponButtons()
        {
            for (int i = 0; i < SlotOrder.Length && i < weaponButtons.Count; i++)
            {
                var weaponType = GetSlotWeaponType(i);
                var button = weaponButtons[i];

                button.onClick.RemoveAllListeners();

                if (ownedWeapons.TryGetValue(weaponType, out var weapon))
                {
                    button.gameObject.SetActive(true);
                    var label = button.GetComponentInChildren<TextMeshProUGUI>();
                    if (label) SlotUtility.GetWeaponName(weapon, label);
                    weaponImages[i].sprite = SlotUtility.GetWeaponSprite(weapon);
                    weaponImages[i].color = Color.white;
                    button.onClick.AddListener(() => ShowWeapon(weaponType));
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }
        }

        private void ShowWeapon(WeaponType weaponType)
        {
            foreach (var go in weaponPreviewMap.Values)
                go.SetActive(false);

            if (!ownedWeapons.TryGetValue(weaponType, out var weapon))
            {
                titleText.text = descriptionText.text = "";
                UpdateStats(0, 0, 0, 0, 0);
                currentWeaponType = null;
                return;
            }

            var stat = SlotUtility.GetWeaponStat(weapon);
            UpdateStats(stat.Damage, stat.MaxAmmoCountInMagazine, stat.Rpm, stat.Recoil, stat.Weight);
            SlotUtility.GetWeaponName(weapon, titleText);
            SlotUtility.GetWeaponDescription(weapon, descriptionText);
            if (weaponPreviewMap.TryGetValue(weaponType, out var previewGo))
                previewGo.SetActive(true);
            currentWeaponType = weaponType;
        }
        
        public void RefreshInventoryUI()
        {
            SyncOwnedWeapons();
            CalculateMaxStats();
            InitializeWeaponButtons();
            
            if (currentWeaponType.HasValue && ownedWeapons.ContainsKey(currentWeaponType.Value))
                ShowWeapon(currentWeaponType.Value);
            else
            {
                WeaponType? firstOwned = null;
                for (int i = 0; i < SlotOrder.Length; i++)
                {
                    var t = GetSlotWeaponType(i);
                    if (!ownedWeapons.ContainsKey(t)) continue;
                    firstOwned = t;
                    break;
                }
                if (firstOwned.HasValue)
                    ShowWeapon(firstOwned.Value);
                else
                {
                    foreach (var go in weaponPreviewMap.Values) go.SetActive(false);
                    titleText.text = descriptionText.text = "";
                    UpdateStats(0,0,0,0,0);
                    currentWeaponType = null;
                }
            }
        }
        
        private void UpdateStats(int damage, int ammoCount, float rpm, float recoil, float weight)
        {
            damageSlider.value = (maxDamage > 0) ? damage / (float)maxDamage : 0f;
            rpmSlider.value = (maxRPM > 0) ? rpm / maxRPM : 0f;
            recoilSlider.value = (maxRecoil > 0) ? recoil / maxRecoil : 0f;
            ammoSlider.value = (maxAmmo > 0) ? ammoCount / (float)maxAmmo : 0f;
            weightSlider.value = (maxWeight > 0) ? weight / maxWeight : 0f;

            damageText.text = damage.ToString();
            rpmText.text = Mathf.RoundToInt(rpm).ToString();
            recoilText.text = Mathf.RoundToInt(recoil).ToString();
            ammoText.text = ammoCount.ToString();
            weightText.text = weight.ToString("F1");
        }

        private IEnumerator HideCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            base.Hide();
        }
    }
}