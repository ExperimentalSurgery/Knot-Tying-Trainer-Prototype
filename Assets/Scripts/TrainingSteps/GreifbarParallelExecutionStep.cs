using System.Collections;

using NMY.VirtualRealityTraining.Steps;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NMY.VirtualRealityTraining.Steps.VirtualAssistant;
using UnityEngine;
using UnityEngine.Localization;

namespace DFKI.NMY
{
    // Override of ParallelExecutionStep for potential extensions in future releases
    public class GreifbarParallelExecutionStep : ParallelExecutionStep {
        
             [Header("Hand Highlight Config")]
         [SerializeField] private List<FingerHighlightContainer> highlights = new List<FingerHighlightContainer>();

        
        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
        
            UserInterfaceManager.instance.ResetFingerHighlights();
            Debug.Log("FingerHighlight");
             // Hand Highlighting
            foreach (FingerHighlightContainer highlightConfig in highlights) {
                UserInterfaceManager.instance.FingerHighlight(highlightConfig);
            }
        }


        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
            UserInterfaceManager.instance.ResetFingerHighlights();
        }


    }
}
