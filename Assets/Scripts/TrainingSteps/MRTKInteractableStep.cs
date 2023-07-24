using System.Threading;
using Cysharp.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace DFKI.NMY
{
    public class MRTKInteractableStep : GreifbarBaseStep {

        
        [Header("MRTK Interactable Step")]
        [SerializeField] private GreifbarMRTKInteractable interactable;

        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            interactable.OnClick.AddListener(OnInteractableClicked);
            interactable.ShowHiglighter();
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
        }


    }
}
