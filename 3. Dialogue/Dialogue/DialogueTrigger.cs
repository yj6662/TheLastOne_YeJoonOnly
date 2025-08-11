using System;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private int dialogueKey;
        [SerializeField] private bool triggerOnce = true;

        private bool hasTriggered = false;

        private void OnEnable()
        {
            
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered && triggerOnce) return;
            if (!other.CompareTag("Player")) return;

            int key = dialogueKey;
            if (CoreManager.Instance.dialogueManager.HasPlayed(key)) return;

            hasTriggered = true;
            CoreManager.Instance.dialogueManager.TriggerDialogue(key);
            CoreManager.Instance.dialogueManager.MarkAsPlayed(key);
        }
    }
}