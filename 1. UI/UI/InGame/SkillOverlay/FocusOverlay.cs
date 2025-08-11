using System.Collections;
using TMPro;
using UnityEngine;

namespace _1.Scripts.UI.InGame.SkillOverlay
{
    public class FocusOverlay : MonoBehaviour
    {
        [SerializeField] private RectTransform leftPanel;
        [SerializeField] private RectTransform rightPanel;
        [SerializeField] private GameObject focusMatrixPrefab;
        [SerializeField] private int columnCount = 10;
        [SerializeField] private float minSpeed = 80f, maxSpeed = 160f;
        [SerializeField] private float charChangeInterval = 0.05f;
        [SerializeField] private float minAlpha = 0.1f, maxAlpha = 0.4f;
        [SerializeField] private float minFontSize = 10, maxFontSize = 50;

        private Coroutine[] leftCoroutines;
        private Coroutine[] rightCoroutines;
        private bool isEffectRunning = false;

        private void Awake()
        {
            leftCoroutines = new Coroutine[columnCount];
            rightCoroutines = new Coroutine[columnCount];
            KillAllMatrixObjects();
        }

        public void SafeShow()
        {
            if (isEffectRunning) return;
            gameObject.SetActive(true);
            RestartEffect();
        }
        public void SafeHide()
        {
            StopEffect();
            gameObject.SetActive(false);
        }

        public void RestartEffect()
        {
            StopEffect();
            isEffectRunning = true;
            for (int i = 0; i < columnCount; i++)
                leftCoroutines[i] = StartCoroutine(SpawnFocusMatrix(leftPanel));
            for (int i = 0; i < columnCount; i++)
                rightCoroutines[i] = StartCoroutine(SpawnFocusMatrix(rightPanel));
        }

        public void StopEffect()
        {
            isEffectRunning = false;
            if (leftCoroutines != null)
            {
                for (int i = 0; i < leftCoroutines.Length; i++)
                    if (leftCoroutines[i] != null)
                    {
                        StopCoroutine(leftCoroutines[i]);
                        leftCoroutines[i] = null;
                    }
            }
            if (rightCoroutines != null)
            {
                for (int i = 0; i < rightCoroutines.Length; i++)
                    if (rightCoroutines[i] != null)
                    {
                        StopCoroutine(rightCoroutines[i]);
                        rightCoroutines[i] = null;
                    }
            }
            KillAllMatrixObjects();
        }

        private void KillAllMatrixObjects()
        {
            foreach (Transform child in leftPanel)
                Destroy(child.gameObject);
            foreach (Transform child in rightPanel)
                Destroy(child.gameObject);
        }

        private IEnumerator SpawnFocusMatrix(RectTransform panel)
        {
            float width = panel.rect.width;
            float height = panel.rect.height;

            while (isEffectRunning)
            {
                float x = Random.Range(-width / 2, width / 2);
                GameObject go = Instantiate(focusMatrixPrefab, panel);
                var rect = go.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(x, height + 50);
                var text = go.GetComponent<TextMeshProUGUI>();
                text.fontSize = Random.Range(minFontSize, maxFontSize);
                Color c = text.color;
                c.a = Random.Range(minAlpha, maxAlpha);
                text.color = c;
                float speed = Random.Range(minSpeed, maxSpeed);
                float y = height + 50;
                float charChangeTimer = 0f;
                while (y > -50 && isEffectRunning)
                {
                    y -= speed * Time.deltaTime;
                    rect.anchoredPosition = new Vector2(x, y);
                    charChangeTimer += Time.deltaTime;
                    if (charChangeTimer > charChangeInterval)
                    {
                        text.text = GetRandomBinaryString(Random.Range(1, 20));
                        charChangeTimer = 0f;
                    }
                    yield return null;
                }
                Destroy(go);
                yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
            }
        }
        private string GetRandomBinaryString(int len)
        {
            var str = "";
            for (int i = 0; i < len; i++)
                str += Random.value < 0.5f ? "0" : "1";
            return str;
        }
    }
}
