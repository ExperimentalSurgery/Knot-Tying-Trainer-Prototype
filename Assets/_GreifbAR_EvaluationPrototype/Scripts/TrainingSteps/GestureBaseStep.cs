using System.Collections;
using System.Collections.Generic;
using NMY.VTT.Core;
using NMY.VTT.VirtualAssistant;
using UnityEngine;
using UnityEngine.Localization;

namespace DFKI.NMY
{
    public class GestureBaseStep : VTTBaseListStep
    {
        [Header("Gesture Step")] 
        [SerializeField] private AudioCollection textToSpeechData;
        [SerializeField] private LocalizedString stepTitle;
        [SerializeField] private LocalizedString stepDescription;

        protected override void ActivateEnter()
        {
            base.ActivateEnter();
            UserInterfaceManager.instance.UpdateStepInfos(stepTitle,stepDescription);

            AudioClip clip;
            textToSpeechData.TryGetAudio("DE", out clip);
            if (clip) {
                SFXManager.instance.PlayAudio(clip);
            }
        }

        protected override void ActivateImmediatelyEnter()
        {
            base.ActivateImmediatelyEnter();
            UserInterfaceManager.instance.UpdateStepInfos(stepTitle,stepDescription);
            AudioClip clip;
            textToSpeechData.TryGetAudio("DE", out clip);
            if (clip) {
                SFXManager.instance.PlayAudio(clip);
            }
        }

        protected override void DeactivateEnter()
        {
            base.DeactivateEnter();
            SFXManager.instance.StopAudio();
        }

        protected override void DeactivateImmediatelyEnter()
        {
            base.DeactivateImmediatelyEnter();
            SFXManager.instance.StopAudio();
        }

        [SerializeField] private float nextStepDelay = 2.0f;
        public virtual IEnumerator TriggerDelayedCompletion()
        {
            yield return new WaitForSeconds(nextStepDelay);
            RaiseStepCompletedEvent();
        }

        protected override void OnStepComplete()
        {
            
        }

        protected override void OnStepReset() {
        }
        protected override void OnPause() => GestureSequencePlayer.instance.Pause();
        protected override void OnResume() => GestureSequencePlayer.instance.Resume();
        
    }
}
