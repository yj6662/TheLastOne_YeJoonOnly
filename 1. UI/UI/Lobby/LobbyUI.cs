using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI.Loading;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.Lobby
{
    public class LobbyUI : UIBase
    {
        [Header("Lobby UI")] 
        [SerializeField] private GameObject panel;
        [SerializeField] private Button startButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button exitButton;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            
            startButton.onClick.AddListener(OnStartButtonClicked);
            loadButton.onClick.AddListener(OnLoadButtonClicked);
            exitButton.onClick.AddListener(OnQuitButtonClicked);
            Hide();
        }

        public override void Show()
        {
            panel.SetActive(true);
        }

        public override void Hide()
        {
            panel.SetActive(false);
        }

        private void OnStartButtonClicked()
        {
            CoreManager.Instance.StartGame();
            Hide();
        }
        
        private void OnLoadButtonClicked()
        {
            CoreManager.Instance.ReloadGame();
        }

        private void OnQuitButtonClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}