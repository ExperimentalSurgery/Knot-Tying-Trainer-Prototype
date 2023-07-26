using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NMY.VirtualRealityTraining.Steps.VirtualAssistant;
using UnityEngine;
using UnityEngine.Localization;

namespace DFKI.NMY
{
    public class GreifbarVirtualAssistantStep : VirtualAssistantSpeakStep {
    
         [Header("Localized Step Data")] 
         [SerializeField] private LocalizedString stepTitle;
         [SerializeField] private LocalizedString stepDescription;
         
         
         [Header("Hand Highlight Config")]
         [SerializeField] private List<FingerHighlightContainer> highlights = new List<FingerHighlightContainer>();

    
         public LocalizedString StepTitle{
            get => stepTitle;
            set => stepTitle = value;
            }

        public LocalizedString StepDescription{
            get => stepDescription;
            set => stepDescription = value;
        }


        protected override async UniTask ClientStepActionAsync(CancellationToken ct)
        {
            base.ClientStepActionAsync(ct);
            // Hand Highlighting
            foreach (FingerHighlightContainer highlightConfig in highlights) {
                UserInterfaceManager.instance.FingerHighlight(highlightConfig);
            }

        }
        
        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            UserInterfaceManager.instance.ResetFingerHighlights();
        }

        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
            UserInterfaceManager.instance.ResetFingerHighlights();
        }

        

    }
}
