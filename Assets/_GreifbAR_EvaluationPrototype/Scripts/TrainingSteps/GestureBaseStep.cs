using System.Collections;
using System.Collections.Generic;
using NMY.VTT.Core;
using UnityEngine;

namespace DFKI.NMY
{
    public class GestureBaseStep : VTTBaseListStep
    {
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
