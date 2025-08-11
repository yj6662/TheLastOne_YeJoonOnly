using System.Collections;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.Common
{
    public class GameOverUI : UIBase
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button reloadButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private CanvasGroup gameOverTextGroup;
        [SerializeField] private CanvasGroup buttonsGroup;

        private PlayerCondition playerCondition;
        private Coroutine fadeInCoroutine;
        
        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);

            CoreManager.Instance.gameManager.Player.PlayerCondition.OnDeath -= OnPlayerDeath;
            CoreManager.Instance.gameManager.Player.PlayerCondition.OnDeath += OnPlayerDeath;
        }
        
        public override void Show()
        {
            base.Show();
            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
                fadeInCoroutine = null;
            }
            fadeInCoroutine = StartCoroutine(FadeInCoroutine());
        }

        public override void Hide()
        {
            if (fadeInCoroutine != null) StopCoroutine(fadeInCoroutine);
            panel.SetActive(false);
            canvasGroup.alpha = 0;
            gameOverTextGroup.alpha = 0;
            buttonsGroup.alpha = 0;
        }

        private void Awake()
        {
            reloadButton.onClick.AddListener(() => { Hide(); CoreManager.Instance.ReloadGame(); });
            quitButton.onClick.AddListener(() => { Hide(); CoreManager.Instance.MoveToIntroScene(); });
            Hide();
        }
        private void OnDestroy()
        {
            if (playerCondition) playerCondition.OnDeath -= OnPlayerDeath;
        }

        private void OnPlayerDeath()
        {
            Show();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        private IEnumerator FadeInCoroutine()
        {
            if (!gameObject.activeInHierarchy) yield break;
            yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0.3f, 1f, 3.0f));
            yield return StartCoroutine(FadeCanvasGroup(gameOverTextGroup, 0f, 1f, 1.0f));
            yield return StartCoroutine(FadeCanvasGroup(buttonsGroup, 0f, 1f, 1.0f));
        }
        
        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
        {
            cg.alpha = from;
            cg.gameObject.SetActive(true);
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                cg.alpha = Mathf.Lerp(from, to, time / duration);
                yield return null;
            }
            cg.alpha = to;
        }
    }
}