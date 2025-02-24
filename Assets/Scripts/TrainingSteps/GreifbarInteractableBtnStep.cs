using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit;

namespace DFKI.NMY
{
    public class GreifbarInteractableBtnStep : GreifbarBaseStep {

        
        [Header("Button Interactable Step")]
        [SerializeField] private GreifbARWorldSpaceButton interactionButtonXR;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(interactionButtonXR,"button reference missing "+this.gameObject.name);
        }

        protected override async UniTask ClientStepActionAsync(CancellationToken ct)
        {
            FinishedCriteria = false;
            interactionButtonXR.Interactable.selectEntered.AddListener(OnXRButtonPressed);
            interactionButtonXR.Highlight(true);
            await base.ClientStepActionAsync(ct);

        }

        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            FinishedCriteria = false;
        }

        private void OnXRButtonPressed(SelectEnterEventArgs args)
        {
            interactionButtonXR.Interactable.selectEntered.RemoveListener(OnXRButtonPressed);
            interactionButtonXR.Highlight(false);
            FinishedCriteria = true;
        }


        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
            if (interactionButtonXR) {
                interactionButtonXR.Interactable.selectEntered.RemoveListener(OnXRButtonPressed);
                interactionButtonXR.Highlight(false);
            }
        }


    }
}
