using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Subs;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame.HUD
{
    public class BleedOverlayUI : UIBase
    {
        [SerializeField] private Image bleedImage;
        [SerializeField] private float bleedDuration = 1f;
        
        private Coroutine bleedRoutine;

        public void Flash()
        {
            if (bleedRoutine != null)
            {
                StopCoroutine(bleedRoutine);
                bleedRoutine = null;
            }
            bleedImage.color = new Color(bleedImage.color.r, bleedImage.color.g, bleedImage.color.b, 0.3f);
            gameObject.SetActive(true);
            bleedRoutine = StartCoroutine(BleedRoutine());
        }

        private IEnumerator BleedRoutine()
        {
            float t = 0f;
            float startAlpha = bleedImage.color.a;
            while (t < bleedDuration)
            {
                t += Time.unscaledDeltaTime;
                bleedImage.color = new Color(bleedImage.color.r, bleedImage.color.g, bleedImage.color.b, Mathf.Lerp(startAlpha, 0f, t / bleedDuration));
                yield return null;
            }
            bleedImage.color = new Color(bleedImage.color.r, bleedImage.color.g, bleedImage.color.b, 0f);
            bleedRoutine = null;
            gameObject.SetActive(false);
        }

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            if (bleedRoutine != null)
            {
                StopCoroutine(bleedRoutine);
                bleedRoutine = null;
            }
            bleedImage.color = new Color(bleedImage.color.r, bleedImage.color.g, bleedImage.color.b, 0f);
        }

        public override void Hide()
        {
            base.Hide();
            if (bleedRoutine != null)
            {
                StopCoroutine(bleedRoutine);
                bleedRoutine = null;
            }
            bleedImage.color = new Color(bleedImage.color.r, bleedImage.color.g, bleedImage.color.b, 0f);
        }

        public override void ResetUI()
        {
            base.ResetUI();
            if (bleedRoutine != null)
            {
                StopCoroutine(bleedRoutine);
                bleedRoutine = null;
            }
            bleedImage.color = new Color(bleedImage.color.r, bleedImage.color.g, bleedImage.color.b, 0f);
            gameObject.SetActive(false);
        }
    }
}