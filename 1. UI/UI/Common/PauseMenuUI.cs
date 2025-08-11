using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.Common
{
    public class PauseMenuUI : UIBase
    {
        [Header("PauseMenu Elements")]
        [SerializeField] private GameObject panel;
        [SerializeField] private CanvasGroup settingPanel;
        [SerializeField] private Animator settingAnimator;
        [SerializeField] private CanvasGroup tutorialPanel;
        [SerializeField] private Animator tutorialAnimator;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button reloadButton;
        [SerializeField] private Button quitButton;
        
        private PauseHandler pauseHandler;
        public CanvasGroup SettingPanel => settingPanel;
        public Animator SettingAnimator => settingAnimator;
        public CanvasGroup TutorialPanel => tutorialPanel;
        public Animator TutorialAnimator => tutorialAnimator;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            
            pauseHandler = GetComponent<PauseHandler>();
            pauseHandler.Initialize(this);
            
            resumeButton.onClick.RemoveAllListeners();
            reloadButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(() =>
            {
                pauseHandler.TogglePause();
                pauseHandler.ClosePausePanel();
            });

            reloadButton.onClick.AddListener(() =>
            {
                pauseHandler.TogglePause();
                CoreManager.Instance.ReloadGame();
            });

            quitButton.onClick.AddListener(() =>
            {
                pauseHandler.TogglePause();
                CoreManager.Instance.MoveToIntroScene();
            });
            gameObject.SetActive(false);
        }
        
        public override void Show() {  panel.SetActive(true); }

        public override void Hide() { panel.SetActive(false); }
        
        public override void ResetUI() { Hide(); }
    }
}