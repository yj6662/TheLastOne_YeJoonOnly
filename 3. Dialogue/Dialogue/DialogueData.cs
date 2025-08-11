using System;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI.InGame.Dialogue;
using UnityEngine;
using UnityEngine.Localization;

namespace _1.Scripts.Dialogue
{
    [Serializable]
    public struct DialogueData
    {
        public string Speaker;
        public LocalizedString Message;
        public SpeakerType SpeakerType;
        public LocalizedAsset<AudioClip> voiceClip;
    }
}