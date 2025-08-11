using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Dialogue;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Sound;
using Michsky.UI.Shift;
using TMPro;
using UnityEngine;
using UIManager = _1.Scripts.Manager.Subs.UIManager;

namespace _1.Scripts.UI.InGame.Dialogue
{
    public enum SpeakerType
    {
        Ally,
        Enemy,
        Player,
        None
    }
    
    public class DialogueUI : UIBase
    {
        [Header("Dialogue UI")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private UIManagerText nameTextUIManager;
        [SerializeField] private UIManagerText dialogueTextUIManager;
        [SerializeField] private UIManagerImage frameUIManager;
        
        private Coroutine dialogueRoutine;
        private Coroutine fadeRoutine;
        private SoundPlayer currentVoicePlayer;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            ClearTexts();
            gameObject.SetActive(false);
        }

        public override void Show()
        {
            if (fadeRoutine != null) { StopCoroutine(fadeRoutine); fadeRoutine = null; }
            canvasGroup.alpha = 0;
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            StopDialogueRoutine();
            StopVoice();
            ResetUI();
            
            if (!gameObject.activeInHierarchy)
            {
                if (canvasGroup) canvasGroup.alpha = 0;
                gameObject.SetActive(false);
                return;
            }
            if (fadeRoutine != null) { StopCoroutine(fadeRoutine); fadeRoutine = null; }
            fadeRoutine = StartCoroutine(FadeOut(0.15f));
        }
        public override void ResetUI()
        {
            StopDialogueRoutine();
            ClearTexts();
            if (fadeRoutine != null) { StopCoroutine(fadeRoutine); fadeRoutine = null; }
            canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }
        
        private void ClearTexts()
        {
            nameText.text = "";
            dialogueText.text = "";
        }
        
        private void StopDialogueRoutine()
        {
            if (dialogueRoutine == null) return;
            StopCoroutine(dialogueRoutine);
            dialogueRoutine = null;
        }
        
        private void StopVoice()
        {
            if (!currentVoicePlayer) return;
            currentVoicePlayer.Stop();
            currentVoicePlayer = null;
        }
        
        public void ShowSequence(List<DialogueData> sequence)
        {
            if (CoreManager.Instance.gameManager.IsGamePaused) return;
            StopDialogueRoutine();
            StopVoice();
            if (sequence == null || sequence.Count == 0) return;
            if (fadeRoutine != null) { StopCoroutine(fadeRoutine); fadeRoutine = null; }

            gameObject.SetActive(true);
            canvasGroup.alpha = 0;
            ClearTexts();
            dialogueRoutine = StartCoroutine(PlayDialogueSequence(sequence, 0));
        }

        private IEnumerator PlayDialogueSequence(List<DialogueData> sequence, int index)
        {
            if (CoreManager.Instance.gameManager.IsGamePaused)
            {
                ResetUI();
                yield break;
            }
            
            if (index >= sequence.Count)
            {
                yield return StartCoroutine(FadeOut(0.15f));
                gameObject.SetActive(false);
                yield break;
            }

            var data = sequence[index];
            nameText.text = data.Speaker;
            
            bool isTextReady = false;
            string messageValue = "";

            data.Message.StringChanged += value =>
            {
                messageValue = value; 
                isTextReady = true;
            };
            while (!isTextReady) yield return null;
            
            bool isAudioReady = false;
            AudioClip voiceClip = null;
            data.voiceClip.AssetChanged += clip =>
            {
                voiceClip = clip;
                isAudioReady = true;
            };
            while (!isAudioReady) yield return null;


            switch (data.SpeakerType)
            {
                case SpeakerType.Enemy:
                    nameTextUIManager.colorType = UIManagerText.ColorType.Negative;
                    dialogueTextUIManager.colorType = UIManagerText.ColorType.Negative;
                    frameUIManager.colorType = UIManagerImage.ColorType.Negative;
                    break;
                case SpeakerType.Ally:
                    nameTextUIManager.colorType = UIManagerText.ColorType.Primary;
                    dialogueTextUIManager.colorType = UIManagerText.ColorType.Primary;
                    frameUIManager.colorType = UIManagerImage.ColorType.Primary;
                    break;
                case SpeakerType.Player:
                    nameTextUIManager.colorType = UIManagerText.ColorType.Secondary;
                    dialogueTextUIManager.colorType = UIManagerText.ColorType.Secondary;
                    frameUIManager.colorType = UIManagerImage.ColorType.Secondary;
                    break;
                case SpeakerType.None:
                default:
                    break;
            }
            
            yield return StartCoroutine(FadeIn(0.15f));

            StopVoice();
            if (voiceClip)
            {
                var obj = CoreManager.Instance.objectPoolManager.Get("SoundPlayer");
                if (obj && obj.TryGetComponent(out SoundPlayer player))
                {
                    player.Play2D(voiceClip, voiceClip.length, 1.0f);
                    currentVoicePlayer = player;
                }
            }

            dialogueText.text = "";
            
            foreach (char c in messageValue)
            {
                dialogueText.text += c;
                CoreManager.Instance.soundManager.PlayUISFX(SfxType.TypeWriter);
                yield return new WaitForSecondsRealtime(0.01f);
            }

            float waitTime = (voiceClip) ? voiceClip.length : 1.5f;
            yield return new WaitForSecondsRealtime(waitTime);

            for (int i = dialogueText.text.Length - 1; i >= 0; i--)
            {
                dialogueText.text = dialogueText.text.Substring(0, i);
                yield return new WaitForSecondsRealtime(0.01f);
            }

            yield return PlayDialogueSequence(sequence, index + 1);
        }

        private IEnumerator FadeIn(float duration)
        {
            float t = 0;
            while (t < duration)
            {
                canvasGroup.alpha = Mathf.Lerp(0, 1, t / duration);
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1;
        }

        private IEnumerator FadeOut(float duration)
        {
            float t = 0;
            while (t < duration)
            {
                canvasGroup.alpha = Mathf.Lerp(1, 0, t / duration);
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }
    }
}