using _1.Scripts.Manager.Core;
using _1.Scripts.UI.Common;
using _1.Scripts.UI.InGame.Modification;
using Michsky.UI.Shift;
using UnityEngine;

namespace _1.Scripts.UI
{
    public class PauseHandler : MonoBehaviour
    {
        [SerializeField] private BlurManager blurMgr;
        [SerializeField] private Animator pauseAnimator;
        
        [Header("Setting Panel")]
        [SerializeField] private CanvasGroup settingPanel;
        [SerializeField] private Animator settingAnimator;
        
        [Header("Tutorial Panel")]
        [SerializeField] private CanvasGroup tutorialPanel;
        [SerializeField] private Animator tutorialAnimator;
        
        private PauseMenuUI pauseMenuUI;
        private CoreManager coreManager;

        private bool isPaused;
        public bool IsPaused => isPaused;
        
        public void Initialize(PauseMenuUI ui)
        {
            coreManager = CoreManager.Instance;
            pauseMenuUI = ui;
        }

        public void TogglePause()
        {
            if (coreManager.uiManager.IsCutscene) return;
            isPaused = !isPaused;
            if (isPaused) Pause();
            else Resume();
        }

        public void ClosePausePanel()
        {
            Resume();
        }

        private void Pause()
        {
            if (!pauseMenuUI || coreManager.uiManager.IsCutscene) return;
            CoreManager.Instance.uiManager.HideUI<ModificationUI>();
            CoreManager.Instance.uiManager.HideHUD();
            CoreManager.Instance.gameManager.PauseGame();
            blurMgr.BlurInAnim();
            pauseMenuUI.Show();
            pauseAnimator.Play("Window In");

            if (settingPanel && settingPanel.alpha > 0f)
            {
                settingAnimator?.Play("Panel Out");
                settingPanel.alpha = 0f;
                settingPanel.interactable = false;
                settingPanel.blocksRaycasts = false;
            }

            if (!tutorialPanel || !(tutorialPanel.alpha > 0f)) return;
            tutorialAnimator?.Play("Panel Out");
            tutorialPanel.alpha = 0f;
            tutorialPanel.interactable = false;
            tutorialPanel.blocksRaycasts = false;
        }

        private void Resume()
        {
            if (!pauseMenuUI || coreManager.uiManager.IsCutscene) return;
            CoreManager.Instance.uiManager.ShowHUD();
            CoreManager.Instance.gameManager.ResumeGame();
            blurMgr.BlurOutAnim();
            pauseAnimator.Play("Window Out");
            pauseMenuUI.Hide();
            
            if (settingPanel && settingPanel.alpha > 0f)
            {
                settingAnimator?.Play("Panel Out");
                settingPanel.alpha = 0f;
                settingPanel.interactable = false;
                settingPanel.blocksRaycasts = false;
            }

            if (!tutorialPanel || !(tutorialPanel.alpha > 0f)) return;
            tutorialAnimator?.Play("Panel Out");
            tutorialPanel.alpha = 0f;
            tutorialPanel.interactable = false;
            tutorialPanel.blocksRaycasts = false;
        }

        public void SetPauseMenuUI(PauseMenuUI ui)
        {
            pauseMenuUI = ui;
            settingPanel = ui.SettingPanel;
            settingAnimator = ui.SettingAnimator;
            tutorialPanel = ui.TutorialPanel;
            tutorialAnimator = ui.TutorialAnimator;
        }
    }
}