using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace DFKI.NMY
{
    public class MRTKInteractableStep : KnotGestureBaseStep {

        [SerializeField] private Interactable _interactable;

        
        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            _interactable.OnClick.AddListener(OnInteractableClicked);
        }

        private void OnInteractableClicked() {
            _interactable.OnClick.RemoveListener(OnInteractableClicked);
            FinishedCriteria = true;
        }

        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
            _interactable.OnClick.RemoveListener(OnInteractableClicked);
        }


    }
}
