using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace DFKI.NMY
{
    public class MRTKInteractableStep : GreifbarBaseStep {

        
        [Header("MRTK Interactable Step")]
        [SerializeField] private GreifbarMRTKInteractable interactable;
        [SerializeField] private GreifbarLeapInteractionButton interactionButton;

        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            interactable.OnClick.AddListener(OnInteractableClicked);
            interactionButton.OnPress-=OnButtonPressed;
            interactionButton.OnPress+=OnButtonPressed;
            interactionButton.ShowHiglighter();
            interactable.ShowHiglighter();

        }

        private void OnButtonPressed()
        {
            interactionButton.OnPress-=OnButtonPressed;
            interactionButton.HideHighlighter();
            FinishedCriteria = true;
        }

        private void OnInteractableClicked() {
            interactable.OnClick.RemoveListener(OnInteractableClicked);
            interactable.HideHighlighter();
            FinishedCriteria = true;
        }

        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
            interactable.OnClick.RemoveListener(OnInteractableClicked);
            interactable.HideHighlighter();
            
            interactionButton.OnPress-=OnButtonPressed;
            interactionButton.HideHighlighter();
        }


    }
}
