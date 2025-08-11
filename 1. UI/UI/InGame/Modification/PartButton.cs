using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Weapon.Scripts.WeaponDetails;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame.Modification
{
    public class PartButton : MonoBehaviour
    {
        public enum SlideDirection { Left, Right }

        [Header("Main")]
        [SerializeField] Button mainButton;
        [SerializeField] Image icon;
        [SerializeField] TextMeshProUGUI label;

        [Header("Aux Buttons")]
        [SerializeField] Button equipButton;
        [SerializeField] Button unEquipButton;
        [SerializeField] RectTransform equipRT;     // 인스펙터에서 '슬롯 안'의 최종 위치로 배치
        [SerializeField] RectTransform unEquipRT;   // 인스펙터에서 '슬롯 안'의 최종 위치로 배치

        [Header("Slide")]
        [SerializeField, Min(0f)] float slideTime = 0.12f;
        [SerializeField, Min(0f)] float gap = 12f;                 // 슬롯 바깥으로 나갈 때 여백(폭+gap)
        [SerializeField] SlideDirection equipDirection = SlideDirection.Right;    // 인스펙터에서 조절
        [SerializeField] SlideDirection unEquipDirection = SlideDirection.Right;  // 인스펙터에서 조절
        [SerializeField] bool fade = true; // 필요없으면 끄기

        // 바인딩 정보 (ModificationUI가 채움)
        public int Index { get; private set; }
        public PartType PartType { get; private set; }
        public int PartId { get; private set; }
        public bool HasPart { get; private set; }
        public bool IsEquipped { get; private set; }

        public event Action<int> OnSelected;
        public event Action<int> OnRequestEquip;
        public event Action<int> OnRequestUnEquip;

        readonly Dictionary<RectTransform, Vector2> basePos = new();

        void Awake()
        {
            if (!mainButton) mainButton = GetComponent<Button>();
            if (!equipRT && equipButton)    equipRT    = (RectTransform)equipButton.transform;
            if (!unEquipRT && unEquipButton) unEquipRT = (RectTransform)unEquipButton.transform;

            mainButton?.onClick.AddListener(() => OnSelected?.Invoke(Index));
            equipButton?.onClick.AddListener(() => OnRequestEquip?.Invoke(Index));
            unEquipButton?.onClick.AddListener(() => OnRequestUnEquip?.Invoke(Index));

            Collapse(true);
        }

        void OnEnable()
        {
            Canvas.ForceUpdateCanvases();
            CacheBase(equipRT);
            CacheBase(unEquipRT);

            HideInside(equipRT);
            HideInside(unEquipRT);
        }

        public void Bind(int index, PartType type, int id, bool hasPart, bool isEquipped, Sprite sp, string labelText)
        {
            Index = index; PartType = type; PartId = id; HasPart = hasPart; IsEquipped = isEquipped;

            gameObject.SetActive(hasPart);

            if (icon)
            {
                icon.sprite = sp;
                icon.color = isEquipped ? new Color(1, 1, 1, 0.5f) : Color.white;
            }
            if (label) label.text = labelText ?? string.Empty;

            Collapse(true);
        }

        public void SetLabel(string text) => label.text = text ?? string.Empty;
        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        public void ExpandForState(bool isEquipped)
        {
            if (!gameObject.activeInHierarchy) return;

            if (isEquipped)
            {
                StartCoroutine(SlideOut(unEquipRT, unEquipDirection));
                StartCoroutine(SlideIn (equipRT,  equipDirection));
            }
            else
            {
                StartCoroutine(SlideOut(equipRT,  equipDirection));
                StartCoroutine(SlideIn (unEquipRT, unEquipDirection));
            }
        }

        public void Collapse(bool instant = false)
        {
            if (instant)
            {
                InstantIn(equipRT);
                InstantIn(unEquipRT);
            }
            else
            {
                StartCoroutine(SlideIn(equipRT,  equipDirection));
                StartCoroutine(SlideIn(unEquipRT, unEquipDirection));
            }
        }


        IEnumerator SlideOut(RectTransform rt, SlideDirection dir)
        {
            if (!rt) yield break;

            CacheBase(rt);
            var baseP = basePos[rt];
            float sign = dir == SlideDirection.Right ? 1f : -1f;
            var outside = baseP + new Vector2(sign * GetOffset(rt), 0f);

            var cg = GetOrAddCanvasGroup(rt);
            rt.gameObject.SetActive(true);
            rt.SetAsLastSibling();
            if (fade) cg.alpha = 0f;

            float t = 0f;
            while (t < slideTime)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / slideTime);
                rt.anchoredPosition = Vector2.Lerp(baseP, outside, k);
                if (fade) cg.alpha = k;
                yield return null;
            }

            rt.anchoredPosition = outside;
            if (fade) cg.alpha = 1f;
        }

        IEnumerator SlideIn(RectTransform rt, SlideDirection dir)
        {
            if (!rt) yield break;

            CacheBase(rt);
            var baseP = basePos[rt];
            float sign = dir == SlideDirection.Right ? 1f : -1f;
            var outside = baseP + new Vector2(sign * GetOffset(rt), 0f);

            var cg = GetOrAddCanvasGroup(rt);
            rt.gameObject.SetActive(true);
            rt.SetAsLastSibling();
            if (fade) cg.alpha = 1f;

            float t = 0f;
            Vector2 start = rt.anchoredPosition;
            while (t < slideTime)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / slideTime);
                rt.anchoredPosition = Vector2.Lerp(start, baseP, k);
                if (fade) cg.alpha = 1f - k;
                yield return null;
            }

            rt.anchoredPosition = baseP;
            if (fade) cg.alpha = 0f;
            rt.gameObject.SetActive(false);
        }

        void InstantIn(RectTransform rt)
        {
            if (!rt) return;
            CacheBase(rt);
            var cg = GetOrAddCanvasGroup(rt);
            rt.anchoredPosition = basePos[rt];
            if (fade) cg.alpha = 0f;
            rt.gameObject.SetActive(false);
        }

        void HideInside(RectTransform rt)
        {
            if (!rt) return;
            CacheBase(rt);
            var cg = GetOrAddCanvasGroup(rt);
            rt.anchoredPosition = basePos[rt];
            if (fade) cg.alpha = 0f;
            rt.gameObject.SetActive(false);
        }

        void CacheBase(RectTransform rt)
        {
            if (!rt) return;
            if (!basePos.ContainsKey(rt)) basePos[rt] = rt.anchoredPosition;
        }

        float GetOffset(RectTransform rt) => (rt ? rt.rect.width : 160f) + gap;

        CanvasGroup GetOrAddCanvasGroup(RectTransform rt)
        {
            var cg = rt.GetComponent<CanvasGroup>();
            if (!cg) cg = rt.gameObject.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}
