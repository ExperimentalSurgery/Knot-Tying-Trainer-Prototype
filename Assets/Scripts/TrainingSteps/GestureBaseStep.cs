using System.Collections;
using System.Collections.Generic;
using NMY.GoogleCloudTextToSpeech;
using NMY.VTT.Core;
using UnityEngine;
using UnityEngine.Localization;

namespace DFKI.NMY
{
    public class GestureBaseStep : VTTBaseListStep
    {
        [Header("Gesture Step")] 
        [SerializeField] private LocalizedString stepTitle;
        [SerializeField] private LocalizedString stepDescription;
        [SerializeField] private LocalizedTextToSpeechItem spokenTextTts;
        protected bool locked = false;


        public LocalizedString StepTitle
        {
            get => stepTitle;
            set => stepTitle = value;
        }

        public LocalizedString StepDescription
        {
            get => stepDescription;
            set => stepDescription = value;
        }

        protected override void ActivateEnter()
        {
            base.ActivateEnter();
            locked = false;
            UserInterfaceManager.instance.UpdateStepInfos(stepTitle,stepDescription);

            if (spokenTextTts && spokenTextTts.audioClip) {
                SFXManager.instance.PlayAudio(spokenTextTts.audioClip);
            }
            
        }

        protected override void ActivateImmediatelyEnter()
        {
            base.ActivateImmediatelyEnter();
            locked = false;
            UserInterfaceManager.instance.UpdateStepInfos(stepTitle,stepDescription);
            //AudioClip clip;
            if (spokenTextTts && spokenTextTts.audioClip) {
                SFXManager.instance.PlayAudio(spokenTextTts.audioClip);
            }
}

protected override void DeactivateEnter()
{
base.DeactivateEnter();
locked = false;
SFXManager.instance.StopAudio();
}

protected override void DeactivateImmediatelyEnter()
{
base.DeactivateImmediatelyEnter();
locked = false;
SFXManager.instance.StopAudio();
}

[SerializeField] private float nextStepDelay = 2.0f;
public virtual IEnumerator TriggerDelayedCompletion()
{
    locked = true;
    yield return new WaitForSeconds(nextStepDelay);
    RaiseStepCompletedEvent();
    locked = false;
}

protected override void OnStepComplete()
{
    locked = false;
}

protected override void OnStepReset() {
}
protected override void OnPause() => GestureSequencePlayer.instance.Pause();
protected override void OnResume() => GestureSequencePlayer.instance.Resume();

}
}
