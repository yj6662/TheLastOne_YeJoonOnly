using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Subs;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _1.Scripts.Sound
{
    [CreateAssetMenu(fileName = "New SoundGroup", menuName = "ScriptableObjects/Sound", order = 0)]
    public class SoundGroupSO : ScriptableObject
    {
        public string groupName;
        public List<AssetReferenceT<AudioClip>> audioClips;

        public AssetReferenceT<AudioClip> GetClip(int index)
        {
            if (audioClips.Count == 0) return null;
            if (index < 0 || index >= audioClips.Count) return null;
            return audioClips[index];
        }
        
        public AssetReferenceT<AudioClip> GetRandomClip()
        {
            if (audioClips.Count == 0) return null;
            int randomIndex = Random.Range(0, audioClips.Count);
            return audioClips[randomIndex];
        }
    }
}