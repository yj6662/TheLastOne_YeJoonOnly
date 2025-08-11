using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using PlayerInput = _1.Scripts.Entity.Scripts.Player.Core.PlayerInput;

namespace _1.Scripts.UI.InGame.HUD
{
    public class CrosshairController : MonoBehaviour
    {
        [SerializeField] private GameObject crosshair;
        [SerializeField] private Image[] crosshairImage;
        [SerializeField] private RectTransform crosshairRectTransform;
        [SerializeField] private float crosshairSize = 1.2f;
        [SerializeField] private float sizeModifyDuration = 0.1f;

        [SerializeField] private Image hitMarker;
        private Coroutine hitMarkerCoroutine;

        private InputAction aimAction;
        private InputAction fireAction;
        private PlayerInput playerInput;
        private Vector3 originalScale;
        
        private Coroutine modifyCoroutine;
        private Coroutine shrinkCoroutine;
        
        private void OnEnable()
        {
            if (playerInput == null)
            {
                playerInput = FindObjectOfType<PlayerInput>();
                
                aimAction = playerInput.PlayerActions.Aim;
                fireAction = playerInput.PlayerActions.Fire;
            }
            aimAction.started += OnAimStarted;
            aimAction.canceled  += OnAimCanceled;
            fireAction.started += OnFirePerformed;
            fireAction.canceled += OnFireCanceled;
        }
        private void Start()
        {
            originalScale = crosshairRectTransform.localScale;
        }
        
        private void OnDisable()
        {
            if (playerInput == null) return;
            
            aimAction.started -= OnAimStarted;
            aimAction.canceled  -= OnAimCanceled;
            fireAction.started -= OnFirePerformed;
            fireAction.canceled -= OnFireCanceled;
        }

        private void OnAimStarted(InputAction.CallbackContext context)
        {
            foreach (var t in crosshairImage)
            {
                t.enabled = false;
            }
        }
        
        private void OnAimCanceled(InputAction.CallbackContext context)
        {
            foreach (var t in crosshairImage)
            {
                t.enabled = true;
            }
        }

        private void OnFirePerformed(InputAction.CallbackContext context)
        {
            if (shrinkCoroutine != null)
            {
                StopCoroutine(shrinkCoroutine);
                shrinkCoroutine = null;
            }

            modifyCoroutine ??= StartCoroutine(ModifyCrosshairSize());
        }
        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            if (modifyCoroutine != null)
            {
                StopCoroutine(modifyCoroutine);
                modifyCoroutine = null;
            }

            shrinkCoroutine ??= StartCoroutine(ShrinkCrosshairSize());
        }

        private IEnumerator ModifyCrosshairSize()
        {
            var rectTransform = crosshairRectTransform;
            Vector3 target = originalScale * crosshairSize;
            float t = 0;

            while (t < sizeModifyDuration)
            {
                t += Time.unscaledDeltaTime;
                rectTransform.localScale = Vector3.Lerp(originalScale, target, t / sizeModifyDuration);
                yield return null;
            }

            modifyCoroutine = null;
        }

        private IEnumerator ShrinkCrosshairSize()
        {
            var rectTransform = crosshairRectTransform;
            Vector3 startSize = rectTransform.localScale;
            float t = 0;

            while (t < sizeModifyDuration)
            {
                t += Time.unscaledDeltaTime;
                rectTransform.localScale = Vector3.Lerp(startSize, originalScale, t / sizeModifyDuration);
                yield return null;
            }
            rectTransform.localScale = originalScale;
            shrinkCoroutine = null;
        }

        public void ShowHitMarker(bool isHeadShot)
        {
            Color color = isHeadShot ? Color.red : Color.white;
            color.a = 0.5f;
            hitMarker.color = color;
            hitMarker.gameObject.SetActive(true);
            
            if (hitMarkerCoroutine != null)
                StopCoroutine(hitMarkerCoroutine);
            hitMarkerCoroutine = StartCoroutine(DisappearCoroutine());
        }

        private IEnumerator DisappearCoroutine()
        {
            float timer = 0f;
            Color startColor = hitMarker.color;
            while (timer < 0.5f)
            {
                timer += Time.unscaledDeltaTime;
                float t = timer / 0.5f;
                hitMarker.color = Color.Lerp(startColor, Color.clear, t);
                yield return null;
            }
            hitMarker.gameObject.SetActive(false);
            hitMarkerCoroutine = null;
        }
    }
}