using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace DFKI.NMY
{
    public class GreifbarFeedbackStep : GreifbarBaseStep
    {
        [SerializeField] private GreifbARWorldSpaceButton continueBtn;
        [SerializeField] private CompletionPanelUI completionPanel;

        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            FindObjectOfType<GreifbarUserPerformanceManager>().UpdateGraphs();
        }

        protected override async UniTask ClientStepActionAsync(CancellationToken ct)
        {
            try
            {
                continueBtn.Interactable.selectEntered.AddListener(OnButtonClicked);
                await base.ClientStepActionAsync(ct);
            }
            catch (OperationCanceledException) {
                RaiseClientStepFinished();
            }
         
        }

        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
        
            continueBtn.Interactable.selectEntered.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked(SelectEnterEventArgs arg0)
        {
            FinishedCriteria = true;
        }
    }
}
