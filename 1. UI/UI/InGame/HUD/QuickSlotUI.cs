using System.Collections;
using _1.Scripts.Item.Common;
using _1.Scripts.Manager.Core;
using Michsky.UI.Shift;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;
using UIManager = _1.Scripts.Manager.Subs.UIManager;

namespace _1.Scripts.UI.InGame.HUD
{
    public class QuickSlotUI : UIBase
    {
        [Header("QuickSlotUI")]
        [SerializeField] private GameObject quickSlotPanel;
        [SerializeField] private Animator quickSlotPanelAnimator;
        [SerializeField] private CanvasGroup quickSlotGroup;
        [SerializeField] private PointerEnterEvents[] slotEvents;
        
        [Header("QuickSlot Elements")] 
        [SerializeField] public Image[] slotIcons;
        [SerializeField] private TextMeshProUGUI[] slotNames;
        [SerializeField] private TextMeshProUGUI[] slotCounts;
        
        private CoreManager coreManager;
        private int currentSlot = -1;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            
            coreManager = CoreManager.Instance;
            quickSlotGroup.alpha = 0;
            quickSlotGroup.blocksRaycasts = false;
            quickSlotPanel.SetActive(false);
            
            for (int i = 0; i < slotEvents.Length; i++)
            {
                int idx = i;
                slotEvents[i].enterEvent.AddListener(() => { currentSlot = idx; });
                slotEvents[i].exitEvent.AddListener(() => { currentSlot = -1; });
            }
            
            gameObject.SetActive(false);
        }

        public override void Show()
        {
            OpenQuickSlot();
        }

        public override void Hide()
        {
            CloseQuickSlot();
        }

        public override void ResetUI()
        {
            currentSlot = -1;
            for (int i = 0; i < slotIcons.Length; i++)
            {
                slotIcons[i].sprite = null;
                slotIcons[i].enabled = false;

                slotCounts[i].text = string.Empty;
                slotCounts[i].gameObject.SetActive(false);
            }

            quickSlotPanel.SetActive(false);
            quickSlotGroup.alpha = 0f;
            quickSlotGroup.blocksRaycasts = false;

            if (quickSlotPanelAnimator) quickSlotPanelAnimator.Rebind();
        }
        
        private void OpenQuickSlot()
        {
            currentSlot = -1;
            quickSlotPanel.SetActive(true);
            quickSlotGroup.alpha = 1;
            quickSlotGroup.blocksRaycasts = true;
            quickSlotPanelAnimator.Play("Panel In");
            foreach (var slot in slotEvents) slot.exitEvent.Invoke();
            
            RefreshQuickSlot();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void CloseQuickSlot()
        {
            if (!quickSlotPanel.activeInHierarchy) return;
            
            quickSlotPanelAnimator.Play("Panel Out");
            quickSlotGroup.alpha = 0;
            quickSlotGroup.blocksRaycasts = false;
            StartCoroutine(CloseQuickSlot_Coroutine());
            
            if (currentSlot != -1) UseSlot(currentSlot);
            else if (currentSlot == -1)
            {
                Service.Log($"선택된 아이템 없음");
            }
            
            RefreshQuickSlot(); 
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        private IEnumerator CloseQuickSlot_Coroutine()
        {
            yield return new WaitForSeconds(0.1f);
            quickSlotPanel.SetActive(false);
        }

        private void UseSlot(int idx)
        {
            Service.Log($"{idx} 번째 슬롯의 아이템 사용");
            CoreManager.Instance.gameManager.Player.PlayerInventory.OnSelectItem((ItemType)currentSlot);
        }

        private void RefreshQuickSlot()
        {
            var items = coreManager.gameManager.Player.PlayerInventory.Items;
            for (int i = 0; i < slotIcons.Length; i++)
            {
                ItemType type = (ItemType)i;
                BaseItem item = null;
                foreach (var it in items)
                {
                    if (it.Key == type)
                    {
                        item = it.Value;
                        Service.Log($"QuickSlot UI : {item.ItemData.NameKey} {item.CurrentItemCount}");
                        break;
                    }
                }

                if (item is { CurrentItemCount: > 0 })
                {
                    slotIcons[i].enabled = true;
                    slotIcons[i].sprite = item.ItemData.Icon;
                    slotCounts[i].text = item.CurrentItemCount.ToString();
                    slotCounts[i].gameObject.SetActive(true);
                    if (slotNames == null || i >= slotNames.Length) continue;
                    var localizedName = new LocalizedString("New Table", item.ItemData.NameKey);
                    int idx = i;
                    localizedName.StringChanged += value =>
                    {
                        slotNames[idx].text = value;
                        slotNames[idx].gameObject.SetActive(true);
                    };
                }
                else
                {
                    slotIcons[i].sprite = null;
                    slotIcons[i].enabled = false;
                    slotCounts[i].text = "";
                    slotCounts[i].gameObject.SetActive(false);

                    if (slotNames == null || i >= slotNames.Length) continue;
                    slotNames[i].text = "";
                    slotNames[i].gameObject.SetActive(false);
                }
            }
        }

    }
}