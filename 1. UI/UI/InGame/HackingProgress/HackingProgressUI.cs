using System.Collections;
using _1.Scripts.Manager.Core;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame.HackingProgress
{
    public class HackingProgressUI : MonoBehaviour
    {
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Animator animator;
        [SerializeField] private float offsetY;

        private Transform target;
        private Transform camera;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            animator.Rebind();
            animator.Update(0);
        }

        private void Start()
        {
            var cam = Camera.main;
            if (cam) camera = cam.transform;
        }

        private void LateUpdate()
        {
            if (!target || !camera) return;
            transform.position = target.position + Vector3.up * offsetY;
            transform.LookAt(camera);
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
            ResetUI();
            gameObject.SetActive(true);
            animator.SetTrigger("Show");
        }
        
        public void SetProgress(float progress)
        {
            progressSlider.value = progress * 100;
        }
        
        public void OnSuccess()
        {
            ResetUI();
            animator.SetTrigger("Success");
            StartCoroutine(DisappearCoroutine(1f));
        }
        
        public void OnFail()
        {
            ResetUI();
            animator.SetTrigger("Fail");
            StartCoroutine(DisappearCoroutine(1f));
        }

        private IEnumerator DisappearCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            CoreManager.Instance.objectPoolManager.Release(gameObject);
        }
        
        private void ResetUI()
        {
            animator.Play("Idle", 0, 0);
            animator.ResetTrigger("Success");
            animator.ResetTrigger("Fail");
            animator.ResetTrigger("Show");
            animator.Update(0);
        }

        public void OnCanceled()
        {
            ResetUI();
            CoreManager.Instance.objectPoolManager.Release(gameObject);
        }
    }
}