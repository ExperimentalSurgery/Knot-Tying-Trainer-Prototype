using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DFKI.NMY
{
    public class GreifbarInteractableBtnStep : GreifbarBaseStep {

        
        [Header("Button Interactable Step")]
        [SerializeField] private GreifbarLeapInteractionButton interactionButton;

        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            if (interactionButton)
            {
                interactionButton.OnPress -= OnButtonPressed;
                interactionButton.OnPress += OnButtonPressed;
                interactionButton.ShowHiglighter();
            }
            else
            {
                FinishedCriteria = true;
            }
            
        }

        private void OnButtonPressed()
        {
                interactionButton.OnPress -= OnButtonPressed;
                interactionButton.HideHighlighter();
                FinishedCriteria = true;
            
        }

        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
            if (interactionButton)
            {
                interactionButton.OnPress -= OnButtonPressed;
                interactionButton.HideHighlighter();
            }
        }


    }
}
