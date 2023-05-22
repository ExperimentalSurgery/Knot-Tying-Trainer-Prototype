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

        protected override void ActivateEnter()
        {
            base.ActivateEnter();
            UserInterfaceManager.instance.UpdateStepInfos(stepTitle,stepDescription);

            if (spokenTextTts && spokenTextTts.audioClip) {
                SFXManager.instance.PlayAudio(spokenTextTts.audioClip);
            }
            
        }

        protected override void ActivateImmediatelyEnter()
        {
            base.ActivateImmediatelyEnter();
            UserInterfaceManager.instance.UpdateStepInfos(stepTitle,stepDescription);
            //AudioClip clip;
            if (spokenTextTts && spokenTextTts.audioClip) {
                SFXManager.instance.PlayAudio(spokenTextTts.audioClip);
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
