using System;
using System.Collections.Generic;
using NMY.GoogleCloudTextToSpeech;
using NMY.VirtualRealityTraining.Steps;
using NMY.VirtualRealityTraining.VirtualAssistant;
using UnityEngine;
using UnityEngine.Localization;

namespace DFKI.NMY
{

    using Cysharp.Threading.Tasks;
    using System.Threading;
    
    public class GreifbarBaseStep : BaseTrainingStep
    {

        [Header("Localized Step Data")] 
        [SerializeField] private LocalizedString stepTitle;
        [SerializeField] private LocalizedString stepDescription;
        
        
        [Header("Hand Highlight Config")]
        [SerializeField] public List<FingerHighlightContainer> highlights = new List<FingerHighlightContainer>();
        
        // privates
        private bool _finishedCriteria = false;
        
        public bool FinishedCriteria
        {
            get => _finishedCriteria;
            set => _finishedCriteria = value;
        }

        public LocalizedString StepTitle
        {
            get => stepTitle;
            set => stepTitle = value;
        }

        public LocalizedString StepDescription
        {
            get => stepDescription;
            set => stepDescription = value;
        }

        
        protected override async UniTask ClientStepActionAsync(CancellationToken ct)
        {
            Debug.Log(this.gameObject.name+" StepActionAsync");
                try {
                    var finishedCriteriaTask = WaitForFinishedCriteria(ct);
                    await UniTask.WhenAny(finishedCriteriaTask);
                    RaiseClientStepFinished();
                }
                catch (OperationCanceledException) {
                    RaiseClientStepFinished();
                }
         
        }

        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            FinishedCriteria = false;
            Debug.Log(this.gameObject.name+" PreStepActionAsync");
            /*UserInterfaceManager.instance.ResetFingerHighlights();
            // Hand Highlighting
                foreach (FingerHighlightContainer highlightConfig in highlights) {
                    UserInterfaceManager.instance.FingerHighlight(highlightConfig);
                }
            */
        }

        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
            Debug.Log(this.gameObject.name+" PostStepAction");
            //UserInterfaceManager.instance.ResetFingerHighlights();
        }
        
        public virtual UniTask WaitForFinishedCriteria(CancellationToken ct)
        {
            return UniTask.WaitUntil(() => _finishedCriteria, cancellationToken: ct);
        }

    }
}