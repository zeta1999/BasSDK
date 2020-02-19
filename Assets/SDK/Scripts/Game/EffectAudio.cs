﻿using UnityEngine;
using System;
using System.Collections;

namespace BS
{
    public class EffectAudio : Effect
    {
        public bool randomPitch;
        public AnimationCurve pitchCurve;
        public AnimationCurve volumeCurve;
        public float loopFadeDelay;

        [NonSerialized]
        public AudioSource audioSource;

        private void Awake()
        {
            audioSource = this.GetComponent<AudioSource>();
            if (!audioSource) audioSource = this.gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1;
            audioSource.playOnAwake = false;
            audioSource.spatialize = true;
        }

        public override void Play()
        {
            CancelInvoke();
            StopAllCoroutines();
            audioSource.loop = step == Step.Loop ? true : false;
            if (randomPitch)
            {
                audioSource.pitch = pitchCurve.Evaluate(UnityEngine.Random.Range(0f, 1f));
            }
            if (step != Step.Loop)
            {
                Invoke("Despawn", audioSource.clip.length + 1);
            }
            audioSource.Play();
        }

        public override void Stop(bool loopOnly = false)
        {
            if (loopFadeDelay > 0)
            {
                StopAllCoroutines();
                StartCoroutine(AudioFadeOut());
            }
            else if (!loopOnly || (loopOnly && step == Step.Loop))
            {
                Despawn();
            }
        }

        public override void SetIntensity(float value, bool loopOnly = false)
        {
            if (!loopOnly || (loopOnly && step == Step.Loop))
            {
                audioSource.pitch = pitchCurve.Evaluate(value);
                audioSource.volume = volumeCurve.Evaluate(value);
            }
        }

        protected IEnumerator AudioFadeOut()
        {
            while (audioSource.volume > 0)
            {
                audioSource.volume -= Time.deltaTime / loopFadeDelay;
                yield return new WaitForEndOfFrame();
            }
            Despawn();
        }

        public override void Despawn()
        {
            CancelInvoke();
            StopAllCoroutines();
            audioSource.Stop();
            if (Application.isPlaying)
            {
                EffectModuleAudio.Despawn(this);
                InvokeDespawnCallback();
            }
        }
    }
}
