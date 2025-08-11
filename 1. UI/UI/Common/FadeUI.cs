using System.Collections;
using _1.Scripts.Manager.Subs;
using UnityEngine;

namespace _1.Scripts.UI.Common
{
    public class FadeUI : UIBase
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Animator animator;
        
        private Coroutine fadeInCoroutine;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            panel.SetActive(false);
        }

        public void FadeOut()
        {
            Service.Log("FadeOut");
            animator.Play("Out");
        }

        public void FadeIn()
        {
            Service.Log("FadeIn");
            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
                fadeInCoroutine = null;
            }
            panel.SetActive(true);
            fadeInCoroutine = StartCoroutine(FadeInCoroutine());
        }
        private IEnumerator FadeInCoroutine()
        {
            if (!gameObject.activeInHierarchy) yield break;
            animator.Play("In");
            yield return new WaitForSecondsRealtime(0.5f);
            panel.SetActive(false);
            yield return null;
        }
    }
}