using System.Collections;
using _1.Scripts.Quests.Core;
using TMPro;
using UnityEngine;

namespace _1.Scripts.UI.InGame.Quest
{
    public class ObjectiveSlot : MonoBehaviour
    {
        [Header("Objective Slot Settings")]
        [SerializeField] private TextMeshProUGUI objectiveDescription;
        [SerializeField] private TextMeshProUGUI objectiveProgress;
        [SerializeField] private float expandDuration = 0.2f;
        
        private Coroutine expandCoroutine;
        private ObjectiveProgress objective;

        public void Initialize(ObjectiveProgress progress)
        {
            objective = progress;
            objectiveDescription.text = objective.data.description;
            UpdateProgress();
            transform.localScale = new Vector3(1, 0, 1);
            gameObject.SetActive(false);
        }

        void OnEnable()
        {
            if (objective is { IsCompleted: true })
            {
                Destroy(gameObject);
                return;
            }
        }
        
        public void UpdateProgress()
        {
            int current = objective.currentAmount;
            int required = objective.data.requiredAmount;
            objectiveProgress.text = $"{current} / {required}";
        }

        public void Expand()
        {
            if (objective is { IsCompleted: true })
            {
                Destroy(gameObject);
                return;
            }
            if (expandCoroutine != null) StopCoroutine(expandCoroutine);
            gameObject.SetActive(true);
            expandCoroutine = StartCoroutine(ExpandCoroutine(0f, 1f));
        }

        public void Collapse()
        {
            if (expandCoroutine != null) StopCoroutine(expandCoroutine);
            expandCoroutine = StartCoroutine(ExpandCoroutine(transform.localScale.y, 0f));
        }

        private IEnumerator ExpandCoroutine(float from, float to)
        {
            float t = 0f;
            Vector3 scale = transform.localScale;
            while (t < expandDuration)
            {
                t += Time.unscaledDeltaTime;
                float y = Mathf.Lerp(from, to, t / expandDuration);
                transform.localScale = new Vector3(scale.x, y, scale.z);
                yield return null;
            }
            transform.localScale = new Vector3(scale.x, to, scale.z);
            if (to == 0) gameObject.SetActive(false);
            expandCoroutine = null;
        }
        
        public void PlayCompleteAndDestroy(float duration = 0.3f)
        {
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                Destroy(gameObject);
                return;
            }

            if (expandCoroutine != null) StopCoroutine(expandCoroutine);
            expandCoroutine = StartCoroutine(FadeAndDestroy(duration));
        }

        private IEnumerator FadeAndDestroy(float duration)
        {
            float t = 0f;
            Vector3 scale = transform.localScale;
            while (t < duration)
            {
                if (!gameObject.activeInHierarchy)
                {
                    Destroy(gameObject); expandCoroutine = null;
                    yield break;
                }
                t += Time.unscaledDeltaTime;
                float y = Mathf.Lerp(1f, 0f, t / duration);
                transform.localScale = new Vector3(scale.x, y, scale.z);
                yield return null;
            }
            Destroy(gameObject);
            expandCoroutine = null;
        }
        
        public bool IsCompleted()
        {
            return objective is { IsCompleted: true };
        }
    }
}