using System.Collections;
using System.Collections.Generic;
using NMY.VTT.Core;
using UnityEngine;
using UnityEngine.Localization;

namespace DFKI.NMY
{
    public class GestureBaseStep : VTTBaseListStep
    {

        [SerializeField] private LocalizedString stepTitle;
        [SerializeField] private LocalizedString stepDescription;


        protected override void ActivateEnter()
        {
            base.ActivateEnter();
            UserInterfaceManager.instance.UpdateStepInfos(stepTitle,stepDescription);
            
        }

        protected override void ActivateImmediatelyEnter()
        {
            base.ActivateImmediatelyEnter();
            UserInterfaceManager.instance.UpdateStepInfos(stepTitle,stepDescription);
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
