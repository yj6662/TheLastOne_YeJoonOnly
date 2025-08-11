using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.UI.Inventory;
using _1.Scripts.Util;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using _1.Scripts.Weapon.Scripts.Hack;
using _1.Scripts.Weapon.Scripts.Melee;
using _1.Scripts.Weapon.Scripts.WeaponDetails;
using Michsky.UI.Shift;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using UIManager = _1.Scripts.Manager.Subs.UIManager;

namespace _1.Scripts.UI.InGame.Modification
{
    public class ModificationUI : UIBase
    {
        private static readonly WeaponType[] SlotOrder =
        {
            WeaponType.Rifle, WeaponType.Pistol, WeaponType.GrenadeLauncher, WeaponType.HackGun
        };

        [SerializeField] private GameObject panel;
        [SerializeField] private Button closeButton;

        [Header("Preview")]
        [SerializeField] private Transform previewSpawnPoint;
        private Dictionary<WeaponType, GameObject> weaponPreviewMap;
        private PreviewWeaponHandler previewWeaponHandler;

        [Header("Part Slots")]
        [SerializeField] private List<PartButton> partSlots;

        private readonly PartType[] allPartTypes = { PartType.Sight, PartType.Sight, PartType.FlameArrester, PartType.Suppressor, PartType.Silencer, PartType.ExtendedMag };

        [Header("Weapon & Part Name")]
        [SerializeField] private TextMeshProUGUI weaponNameText;
        [SerializeField] private TextMeshProUGUI partNameText;
        [SerializeField] private TextMeshProUGUI partDescriptionText;
        [SerializeField] private TextMeshProUGUI requiredText;

        [Header("Part Highlight Material")]
        [SerializeField] private Material partHighlightMaterial;

        [Header("Stats")]
        [SerializeField] private Slider damageSlider, rpmSlider, recoilSlider, weightSlider, ammoSlider;
        [SerializeField] private TextMeshProUGUI damageText, rpmText, recoilText, weightText, ammoText;
        
        [Header("Apply Modal")]
        [SerializeField] private ModalWindowManager modalWindowManager;
        [SerializeField] private GameObject applyModal;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        [Header("Buttons")]
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;

        private Dictionary<WeaponType, BaseWeapon> ownedWeapons = new();
        private int currentWeaponIdx;
        private WeaponType CurrentWeaponType => GetSlotWeaponType(currentWeaponIdx);
        private BaseWeapon CurrentWeapon => ownedWeapons.GetValueOrDefault(CurrentWeaponType);
        
        private Dictionary<(PartType, int), WeaponPartData> partDataMap;
        private WeaponPartData selectedPartData;
        private PartType? selectedPartType;
        private int selectedPartId;

        private PlayerCondition playerCondition;
        private PlayerWeapon playerWeapon;

        private PartType? lastHighlightedPartType;
        private Material lastSelectedPartMaterial = null;

        private struct SlotInfo { public bool hasPart; public bool isEquipped; public PartType type; public int id; }
        private readonly Dictionary<int, SlotInfo> slotInfoMap = new();

        private enum PendingActionType { None, Equip, UnEquip, Forge }
        private PendingActionType pendingAction = PendingActionType.None;
        private PartType pendingType;
        private int pendingId;

        private int expandedIdx = -1;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);

            playerCondition = CoreManager.Instance.gameManager.Player.PlayerCondition;
            playerWeapon = CoreManager.Instance.gameManager.Player.PlayerWeapon;

            weaponPreviewMap = new();
            foreach (Transform child in previewSpawnPoint)
            {
                var handler = child.GetComponent<PreviewWeaponHandler>();
                if (handler) weaponPreviewMap[handler.weaponType] = child.gameObject;
                child.gameObject.SetActive(false);
            }

            CacheAllPartData();

            for (int i = 0; i < partSlots.Count; i++)
            {
                int idx = i;
                if (!partSlots[idx]) continue;
                partSlots[idx].OnSelected += OnSlotSelected;
                partSlots[idx].OnRequestEquip += OnSlotEquip;
                partSlots[idx].OnRequestUnEquip += OnSlotUnEquip;
            }

            // 상단 버튼
            closeButton.onClick.AddListener(Hide);
            applyButton.onClick.AddListener(OnApplyButtonClicked_ForgeOnly);
            confirmButton.onClick.AddListener(OnApplyConfirmed);
            cancelButton.onClick.AddListener(HideModal);
            prevButton.onClick.AddListener(OnPrevWeaponClicked);
            nextButton.onClick.AddListener(OnNextWeaponClicked);

            Hide();
            HideModal();
            ResetUI();
        }

        private void Update()
        {
            if (!panel.activeInHierarchy) return;

            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (applyModal.activeInHierarchy) { HideModal(); return; }
            if (expandedIdx != -1) { partSlots[expandedIdx]?.Collapse(); expandedIdx = -1; return; }
            Hide();
        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveListener(Hide);
            applyButton.onClick.RemoveListener(OnApplyButtonClicked_ForgeOnly);
            confirmButton.onClick.RemoveListener(OnApplyConfirmed);
            cancelButton.onClick.RemoveListener(HideModal);
            prevButton.onClick.RemoveListener(OnPrevWeaponClicked);
            nextButton.onClick.RemoveListener(OnNextWeaponClicked);

            foreach (var slot in partSlots)
            {
                if (!slot) continue;
                slot.OnSelected -= OnSlotSelected;
                slot.OnRequestEquip -= OnSlotEquip;
                slot.OnRequestUnEquip -= OnSlotUnEquip;
            }
        }

        public override void Show()
        {
            base.Show();
            playerCondition.OnDisablePlayerMovement();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Refresh();
        }

        public override void Hide()
        {
            panel.SetActive(false);
            gameObject.SetActive(false);
            UnhighlightPart();
            playerCondition.OnEnablePlayerMovement();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void ResetUI()
        {
            foreach (var go in weaponPreviewMap.Values) go.SetActive(false);
            previewWeaponHandler = null;
            selectedPartType = null;
            selectedPartData = null;
            UnhighlightPart();
            ResetStatUI();
            HideModal();
            currentWeaponIdx = 0;

            foreach (var slot in partSlots) slot?.SetVisible(false);

            if (weaponNameText) weaponNameText.text = "";
            if (partNameText) partNameText.text = "";
        }

        private void CacheAllPartData()
        {
            var allParts = CoreManager.Instance.resourceManager.GetAllAssetsOfType<WeaponPartData>();
            partDataMap = allParts.ToDictionary(x => (x.Type, x.Id), x => x);
        }

        private WeaponType GetSlotWeaponType(int slotIdx)
        {
            var role = SlotOrder[slotIdx];
            if (role == WeaponType.Pistol) return ownedWeapons.ContainsKey(WeaponType.SniperRifle) ? WeaponType.SniperRifle : WeaponType.Pistol;
            return role;
        }

        private void Refresh()
        {
            SyncOwnedWeapons();
            if (!ownedWeapons.ContainsKey(GetSlotWeaponType(currentWeaponIdx))) SetCurrentWeapon();
            ShowWeaponPreview(CurrentWeaponType);
            selectedPartType = null;
            selectedPartData = null;
            UnhighlightPart();
            GeneratePartSlots();
            UpdateStatUI();
            UpdateNameUI();
        }

        private void SyncOwnedWeapons()
        {
            ownedWeapons.Clear();
            if (!playerWeapon) return;

            foreach (var (type, weapon) in playerWeapon.Weapons)
            {
                if (!weapon) continue;
                if (!playerWeapon.AvailableWeapons.TryGetValue(type, out var unlocked) || !unlocked) continue;
                if (type == WeaponType.Punch) continue;
                ownedWeapons[type] = weapon;
            }
        }

        private void SetCurrentWeapon()
        {
            for (int i = 0; i < SlotOrder.Length; i++)
            {
                if (!ownedWeapons.ContainsKey(SlotOrder[i])) continue;
                currentWeaponIdx = i;
                return;
            }
            currentWeaponIdx = 0;
        }

        private void ShowWeaponPreview(WeaponType weaponType)
        {
            foreach (var go in weaponPreviewMap.Values) go.SetActive(false);
            if (weaponPreviewMap.TryGetValue(weaponType, out var previewGo))
            {
                previewGo.SetActive(true);
                previewWeaponHandler = previewGo.GetComponent<PreviewWeaponHandler>();
            }
            else previewWeaponHandler = null;

            lastHighlightedPartType = null;
            lastSelectedPartMaterial = null;
        }

        private void GeneratePartSlots()
        {
            expandedIdx = -1;
            slotInfoMap.Clear();

            foreach (var slot in partSlots) slot?.SetVisible(false);

            if (!CurrentWeapon)
            {
                applyButton.gameObject.SetActive(true);
                applyButton.interactable = false;
                SetLocalizedTextAsync(requiredText, "Require_SelectWeapon");
                return;
            }

            if (CurrentWeaponType == WeaponType.Pistol)
            {
                foreach (var slot in partSlots) slot?.SetVisible(false);

                applyButton.gameObject.SetActive(true);
                if (IsForgeAvailable(CurrentWeapon))
                {
                    applyButton.interactable = true;
                    SetLocalizedTextAsync(requiredText, "Require_ForgeAvailable");
                }
                else
                {
                    applyButton.interactable = false;
                    SetLocalizedTextAsync(requiredText, "Require_ForgeUnavailable");
                }
                return;
            }

            applyButton.gameObject.SetActive(false);

            var partTypeList = GetPartTypes(CurrentWeaponType);
            bool anyPart = false;

            for (int i = 0; i < partSlots.Count && i < allPartTypes.Length; i++)
            {
                var slot = partSlots[i];
                if (!slot) continue;

                var partType = allPartTypes[i];

                if (partType == PartType.Sight && CurrentWeaponType != WeaponType.Rifle && i > 0)
                {
                    slot.SetVisible(false);
                    continue;
                }

                int partId = (CurrentWeaponType == WeaponType.Rifle && partType == PartType.Sight)
                    ? GetPartId(CurrentWeaponType, partType, i)
                    : GetPartId(CurrentWeaponType, partType);

                bool enabled = partTypeList.Contains(partType);
                bool hasPart = enabled && CurrentWeapon.EquipableWeaponParts.TryGetValue(partId, out var own) && own;
                bool isEquipped = false;

                if (CurrentWeapon is Gun { EquippedWeaponParts: not null } gun)
                    isEquipped = gun.EquippedWeaponParts.TryGetValue(partType, out int eq) && eq == partId;
                else if (CurrentWeapon is HackGun { EquippedWeaponParts: not null } hg)
                    isEquipped = hg.EquippedWeaponParts.TryGetValue(partType, out int eq) && eq == partId;
                else if (CurrentWeapon is GrenadeLauncher { EquippedWeaponParts: not null } gl)
                    isEquipped = gl.EquippedWeaponParts.TryGetValue(partType, out int eq) && eq == partId;

                slotInfoMap[i] = new SlotInfo { hasPart = hasPart, isEquipped = isEquipped, type = partType, id = partId };

                Sprite icon = GetPartSprite(CurrentWeaponType, partType, partId);
                string labelText = partType.ToString();

                if (partDataMap.TryGetValue((partType, partId), out var partData) && !string.IsNullOrEmpty(partData.NameKey))
                {
                    var ls = new LocalizedString("New Table", partData.NameKey);
                    var op = ls.GetLocalizedStringAsync();
                    if (op.IsDone) labelText = op.Result + (CurrentWeaponType == WeaponType.Rifle && partType == PartType.Sight ? $" ({partId})" : "");
                    else op.Completed += h => slot.SetLabel(h.Result + (CurrentWeaponType == WeaponType.Rifle && partType == PartType.Sight ? $" ({partId})" : ""));
                }

                slot.Bind(i, partType, partId, hasPart, isEquipped, icon, labelText);
                anyPart |= hasPart;
            }

            SetLocalizedTextAsync(requiredText, anyPart ? "Require_SelectAvailablePart" : "Require_ModificationUnavailable");
        }

        private List<PartType> GetPartTypes(WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.Rifle:           return new List<PartType> { PartType.FlameArrester, PartType.Silencer, PartType.Suppressor, PartType.Sight };
                case WeaponType.GrenadeLauncher: return new List<PartType> { PartType.Sight };
                case WeaponType.HackGun:         return new List<PartType> { PartType.ExtendedMag };
                default:                         return new List<PartType>();
            }
        }

        private void OnSlotSelected(int idx)
        {
            if (!slotInfoMap.TryGetValue(idx, out var info)) return;

            selectedPartType = info.type;
            selectedPartId = info.id;

            if (!partDataMap.TryGetValue((info.type, info.id), out selectedPartData))
            {
                SetLocalizedTextAsync(requiredText, "Require_NoPartDataFound");
                return;
            }

            HighlightPart(info.type);
            UpdateStatPreview(selectedPartData);
            UpdateNameUI();

            if (expandedIdx != -1 && expandedIdx != idx) partSlots[expandedIdx]?.Collapse();

            if (!info.hasPart) { partSlots[idx]?.Collapse(); expandedIdx = -1; return; }

            partSlots[idx].ExpandForState(info.isEquipped);
            expandedIdx = idx;
        }

        private void OnSlotEquip(int idx)
        {
            if (!slotInfoMap.TryGetValue(idx, out var info)) return;
            pendingAction = PendingActionType.Equip;
            pendingType = info.type;
            pendingId = info.id;
            ShowModal();
        }

        private void OnSlotUnEquip(int idx)
        {
            if (!slotInfoMap.TryGetValue(idx, out var info)) return;
            pendingAction = PendingActionType.UnEquip;
            pendingType = info.type;
            pendingId = info.id;
            ShowModal();
        }

        private void UpdateStatPreview(WeaponPartData partData)
        {
            var s = SlotUtility.GetWeaponStat(CurrentWeapon);

            float rate = Mathf.Clamp01(partData.ReduceRecoilRate);
            var newRecoil = s.Recoil * (1f - rate);

            var newAmmo = s.MaxAmmoCountInMagazine + partData.IncreaseMaxAmmoCountInMagazine;

            recoilText.text = Mathf.RoundToInt(newRecoil).ToString();
            ammoText.text = newAmmo.ToString();

            recoilSlider.value = Mathf.Clamp01(newRecoil / 100f);
            ammoSlider.value = Mathf.Clamp01((float)newAmmo / 60f);
        }

        private Sprite GetPartSprite(WeaponType weaponType, PartType partType, int partId)
        {
            return (partDataMap != null && partDataMap.TryGetValue((partType, partId), out var partData)) ? partData.Icon : null;
        }

        private int GetPartId(WeaponType weaponType, PartType partType, int buttonIdx = 0)
        {
            switch (weaponType)
            {
                case WeaponType.Rifle when partType == PartType.Sight: return (buttonIdx == 0) ? 1 : 2;
                case WeaponType.Rifle:
                    switch (partType)
                    {
                        case PartType.ExtendedMag: return 11;
                        case PartType.FlameArrester: return 6;
                        case PartType.Sight: return 1;
                        case PartType.Silencer: return 7;
                        case PartType.Suppressor: return 10;
                    }
                    break;
                case WeaponType.GrenadeLauncher when partType == PartType.Sight: return 5;
                case WeaponType.HackGun:
                    switch (partType)
                    {
                        case PartType.ExtendedMag: return 12;
                        case PartType.Sight: return 4;
                        case PartType.FlameArrester: return 8;
                    }
                    break;
                case WeaponType.Pistol:
                    switch (partType)
                    {
                        case PartType.Sight: return 14;
                        case PartType.Silencer: return 15;
                        case PartType.ExtendedMag: return 13;
                    }
                    break;
            }
            return -1;
        }

        private void HighlightPart(PartType partType)
        {
            UnhighlightPart();
            if (!previewWeaponHandler) return;
            var renderer = previewWeaponHandler.GetRendererOfPart(partType);
            if (!renderer) return;
            lastHighlightedPartType = partType;
            lastSelectedPartMaterial = renderer.material;
            renderer.material = partHighlightMaterial;
        }

        private void UnhighlightPart()
        {
            if (!previewWeaponHandler || lastHighlightedPartType == null) return;
            var renderer = previewWeaponHandler.GetRendererOfPart(lastHighlightedPartType.Value);
            if (renderer && lastSelectedPartMaterial) renderer.material = lastSelectedPartMaterial;
            lastHighlightedPartType = null;
            lastSelectedPartMaterial = null;
        }

        private void OnApplyButtonClicked_ForgeOnly()
        {
            if (!applyButton.interactable) return;
            pendingAction = PendingActionType.Forge;
            ShowModal();
        }

        private void OnApplyConfirmed()
        {
            HideModal();
            bool applied = false;

            switch (pendingAction)
            {
                case PendingActionType.Forge:
                    applied = playerWeapon && playerWeapon.ForgeWeapon();
                    break;
                case PendingActionType.Equip:
                    applied = playerWeapon && playerWeapon.EquipPart(CurrentWeaponType, pendingType, pendingId);
                    break;
                case PendingActionType.UnEquip:
                    applied = playerWeapon && playerWeapon.UnequipPart(CurrentWeaponType, pendingType, pendingId);
                    break;
            }

            pendingAction = PendingActionType.None;

            if (!applied) return;

            if (expandedIdx != -1) { partSlots[expandedIdx]?.Collapse(true); expandedIdx = -1; }
            selectedPartType = null;
            selectedPartData = null;
            UnhighlightPart();
            Refresh();
        }

        private bool IsForgeAvailable(BaseWeapon weapon)
        {
            if (weapon is Gun gun && gun.GunData.GunStat.Type == WeaponType.Pistol)
            {
                int sightId = GetPartId(WeaponType.Pistol, PartType.Sight);
                int extId = GetPartId(WeaponType.Pistol, PartType.ExtendedMag);
                int silencerId = GetPartId(WeaponType.Pistol, PartType.Silencer);

                return gun.EquipableWeaponParts.TryGetValue(sightId, out var hasSight) && hasSight
                    && gun.EquipableWeaponParts.TryGetValue(extId, out var hasExt) && hasExt
                    && gun.EquipableWeaponParts.TryGetValue(silencerId, out var hasSilencer) && hasSilencer;
            }
            return false;
        }

        private void ShowModal()
        {
            modalWindowManager.ModalWindowIn();
        }

        private void HideModal()
        {
            if (!applyModal.activeInHierarchy) return;
            modalWindowManager.ModalWindowOut();
        }


        private void UpdateStatUI()
        {
            if (!CurrentWeapon) { ResetStatUI(); return; }

            var stat = SlotUtility.GetWeaponStat(CurrentWeapon);
            int maxDamage = 1000;
            float maxRPM = 100f;
            float maxRecoil = 100f;
            float maxWeight = 10f;
            int maxAmmo = 60;

            damageSlider.value = (float)stat.Damage / maxDamage;
            rpmSlider.value = stat.Rpm / maxRPM;
            recoilSlider.value = stat.Recoil / maxRecoil;
            weightSlider.value = stat.Weight / maxWeight;
            ammoSlider.value = (float)stat.MaxAmmoCountInMagazine / maxAmmo;

            damageText.text = stat.Damage.ToString();
            rpmText.text = Mathf.RoundToInt(stat.Rpm).ToString();
            recoilText.text = Mathf.RoundToInt(stat.Recoil).ToString();
            weightText.text = stat.Weight.ToString("F1");
            ammoText.text = stat.MaxAmmoCountInMagazine.ToString();
        }

        private void ResetStatUI()
        {
            damageText.text = rpmText.text = recoilText.text = weightText.text = ammoText.text = "";
            damageSlider.value = rpmSlider.value = recoilSlider.value = weightSlider.value = ammoSlider.value = 0f;
        }

        private void UpdateNameUI()
        {
            if (weaponNameText)
            {
                if (CurrentWeapon) SlotUtility.GetWeaponName(CurrentWeapon, weaponNameText);
                else weaponNameText.text = "";
            }

            if (partNameText)
            {
                if (selectedPartData && !string.IsNullOrEmpty(selectedPartData.NameKey))
                    SetLocalizedTextAsync(partNameText, selectedPartData.NameKey);
                else partNameText.text = selectedPartType?.ToString() ?? "";
            }

            if (partDescriptionText)
            {
                if (selectedPartData && !string.IsNullOrEmpty(selectedPartData.DescKey))
                    SetLocalizedTextAsync(partDescriptionText, selectedPartData.DescKey);
                else partDescriptionText.text = "";
            }
        }

        private void SetLocalizedTextAsync(TextMeshProUGUI text, string key)
        {
            if (!text || string.IsNullOrEmpty(key)) return;
            var ls = new LocalizedString("New Table", key);
            var op = ls.GetLocalizedStringAsync();
            if (op.IsDone) text.text = op.Result;
            else op.Completed += h => { if (text) text.text = h.Result; };
        }


        private void OnPrevWeaponClicked()
        {
            for (int i = 1; i <= SlotOrder.Length; i++)
            {
                int idx = (currentWeaponIdx - i + SlotOrder.Length) % SlotOrder.Length;
                if (!ownedWeapons.ContainsKey(GetSlotWeaponType(idx))) continue;
                currentWeaponIdx = idx;
                break;
            }
            Refresh();
        }

        private void OnNextWeaponClicked()
        {
            for (int i = 1; i <= SlotOrder.Length; i++)
            {
                int idx = (currentWeaponIdx + i) % SlotOrder.Length;
                if (!ownedWeapons.ContainsKey(GetSlotWeaponType(idx))) continue;
                currentWeaponIdx = idx;
                break;
            }
            Refresh();
        }
    }
}
