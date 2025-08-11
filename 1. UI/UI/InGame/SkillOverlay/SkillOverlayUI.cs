using _1.Scripts.Manager.Subs;
using UnityEngine;

namespace _1.Scripts.UI.InGame.SkillOverlay
{
    public class SkillOverlayUI : UIBase
    {
        [SerializeField] private FocusOverlay focusOverlay;
        [SerializeField] private InstinctOverlay instinctOverlay;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            HideAll();
        }

        public void ShowFocusOverlay()
        {
            HideAll();
            if (focusOverlay)
            {
                focusOverlay.gameObject.SetActive(true);
                focusOverlay.RestartEffect();
            }
        }

        public void HideFocusOverlay()
        {
            if (focusOverlay)
            {
                focusOverlay.StopEffect();
                focusOverlay.gameObject.SetActive(false);
            }
        }

        public void ShowInstinctOverlay()
        {
            HideAll();
            if (instinctOverlay)
            {
                instinctOverlay.gameObject.SetActive(true);
                instinctOverlay.RestartEffect();
            }
        }

        public void HideInstinctOverlay()
        {
            if (instinctOverlay)
            {
                instinctOverlay.gameObject.SetActive(false);
                instinctOverlay.StopEffect();
            }
        }

        public void HideAll()
        {
            if (focusOverlay) focusOverlay.gameObject.SetActive(false);
            if (instinctOverlay) instinctOverlay.gameObject.SetActive(false);
        }

        public override void Hide()
        {
            base.Hide();
            HideAll();
        }

        public override void ResetUI()
        {
            base.ResetUI();
            HideAll();
        }
    }
}