using NMY.VirtualRealityTraining.Steps;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DFKI.NMY
{
    // Override of ParallelExecutionStep for potential extensions in future releases
    public class GreifbarParallelExecutionStep : ParallelExecutionStep {
        
        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct) {
            Debug.Log(this.gameObject.name+" PretStepAction");
            await base.PreStepActionAsync(ct);
        
            /*UserInterfaceManager.instance.ResetFingerHighlights();
             // Hand Highlighting
            foreach (FingerHighlightContainer highlightConfig in highlights) {
                UserInterfaceManager.instance.FingerHighlight(highlightConfig);
            }
            */
        }

        protected override UniTask ClientStepActionAsync(CancellationToken ct) {
            Debug.Log(this.gameObject.name+" StepAction");
            return base.ClientStepActionAsync(ct);
        }


        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            Debug.Log(this.gameObject.name+" PostStepAction");
            await base.PostStepActionAsync(ct);
            //UserInterfaceManager.instance.ResetFingerHighlights();
        }


    }
}
