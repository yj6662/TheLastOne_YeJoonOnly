using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using UnityEngine;
using UnityEngine.UI;


namespace _1.Scripts.UI.Common
{
    public class EndingCreditUI : UIBase
    {
        [Header("Scroll View")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private CanvasGroup contentCanvasGroup;
        [SerializeField] private float scrollSpeed = 75f;
        [SerializeField] private float scrollDelay = 2f;
        
        [Header("Skip")]
        [SerializeField] private Button skipButton;
        
        [Header("Thank you Message")]
        [SerializeField] private GameObject thankYouPanel;

        private bool isScrolling = false;
        private Coroutine scrollCoroutine;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            Hide();
            skipButton.onClick.AddListener(SkipCredit);
        }

        public override void Show()
        {
            base.Show();

            CoreManager.Instance.uiManager.HideHUD();
            scrollRect.verticalNormalizedPosition = 1f;
            if (thankYouPanel) thankYouPanel.SetActive(false);

            if (skipButton)
            {
                skipButton.gameObject.SetActive(true);
                skipButton.onClick.RemoveAllListeners();
                skipButton.onClick.AddListener(SkipCredit);
            }

            contentCanvasGroup.alpha = 1f;
            scrollCoroutine = StartCoroutine(ScrollCredit());
        }
        public override void Hide()
        {
            base.Hide();
            if (scrollCoroutine != null)
                StopCoroutine(scrollCoroutine);
            if (skipButton)
                skipButton.onClick.RemoveAllListeners();
        }
        IEnumerator ScrollCredit()
        {
            yield return new WaitForSeconds(scrollDelay);

            isScrolling = true;

            float contentHeight = contentRoot.rect.height;
            float viewportHeight = scrollRect.viewport.rect.height;
            float scrollDistance = Mathf.Max(contentHeight - viewportHeight, 1f);

            float pos = 0f;
            while (pos < scrollDistance)
            {
                pos += scrollSpeed * Time.deltaTime;
                float normalized = 1f - Mathf.Clamp01(pos / scrollDistance);
                scrollRect.verticalNormalizedPosition = normalized;
                yield return null;
            }

            scrollRect.verticalNormalizedPosition = 0f;

            isScrolling = false;
            
            if (skipButton)
                skipButton.gameObject.SetActive(false);

            yield return new WaitForSeconds(scrollDelay);
            if (contentCanvasGroup)
                contentCanvasGroup.alpha = 0f;
            
            if (thankYouPanel)
            {
                thankYouPanel.SetActive(true);
                yield return new WaitForSeconds(scrollDelay);
                thankYouPanel.SetActive(false);
            }
            CoreManager.Instance.MoveToIntroScene();
        }
        public void SkipCredit()
        {
            if (!isScrolling) return;
            StopCoroutine(scrollCoroutine);
            scrollRect.verticalNormalizedPosition = 0f;
            if (skipButton)
                skipButton.gameObject.SetActive(false);
            if (thankYouPanel)
                thankYouPanel.SetActive(true);
            CoreManager.Instance.MoveToIntroScene();
        }
    }
}