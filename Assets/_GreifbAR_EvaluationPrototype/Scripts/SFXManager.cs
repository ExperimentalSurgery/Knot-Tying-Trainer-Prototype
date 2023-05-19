using System.Collections;
using System.Collections.Generic;
using NMY;
using UnityEngine;

namespace DFKI.NMY
{
    public class SFXManager : SingletonStartupBehaviour<SFXManager> {
        
        [SerializeField] private AudioSource audioSource;

        public void PlayAudio(AudioClip clip) {
            if (audioSource) {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
        
        public void StopAudio() {
            audioSource.Stop();
        }
        
    }
}
