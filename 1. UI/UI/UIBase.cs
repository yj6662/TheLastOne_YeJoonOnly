using _1.Scripts.Manager.Subs;
using UnityEngine;


namespace _1.Scripts.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        protected UIManager uiManager;

        public virtual void Initialize(UIManager manager, object param = null)
        {
            uiManager = manager;
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }
        
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public virtual void ResetUI(){}
    }
}