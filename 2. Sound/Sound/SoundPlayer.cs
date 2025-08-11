using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Sound
{
    public class SoundPlayer : MonoBehaviour
    {
        private AudioSource audioSource;
        private Coroutine returnCoroutine;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void Play(AudioClip clip, float length = -1f, float volume = 1.0f, float spatialBlend = 0)
        {
            audioSource.clip = clip;
            if (length >= 0f) { float speed = clip.length / length; audioSource.pitch = speed; }
            else audioSource.pitch = 1f;
            audioSource.volume = volume;
            audioSource.spatialBlend = spatialBlend;
            audioSource.Play();
            
            returnCoroutine = StartCoroutine(ReturnToPool());
        }

        public void Play2D(AudioClip clip, float duration, float volume)
        {
            transform.position = Vector3.zero;
            Play(clip, duration, volume, 0.0f);
        }

        public void Play3D(AudioClip clip, float duration, float volume, Vector3 position)
        {
            transform.position = position;
            Play(clip, duration, volume, 1.0f);
        }

        public void Stop()
        {
            audioSource.Stop();
        }

        private IEnumerator ReturnToPool()
        {
            yield return new WaitWhile(() => audioSource.isPlaying);
            CoreManager.Instance.objectPoolManager.Release(gameObject);
        }
        public bool IsPlaying()
        {
            return audioSource && audioSource.isPlaying;
        }
    }
}
