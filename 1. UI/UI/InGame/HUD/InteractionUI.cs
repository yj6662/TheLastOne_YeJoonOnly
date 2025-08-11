using System.Collections;
using UnityEngine;

namespace _1.Scripts.UI.InGame.HUD
{
    public class InteractionUI : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private Coroutine hideRoutine;

        public void Show()
        {
            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
                hideRoutine = null;
            }
            gameObject.SetActive(true);
            animator.ResetTrigger("Hide");
            animator.SetTrigger("Show");
        }

        public void Hide()
        {
            if (hideRoutine != null) return;
            hideRoutine = StartCoroutine(HideCoroutine());
        }

        private IEnumerator HideCoroutine()
        {
            animator.ResetTrigger("Show");
            animator.SetTrigger("Hide");
            yield return new WaitForSeconds(0.5f);
            gameObject.SetActive(false);
            hideRoutine = null;
        }
    }
}