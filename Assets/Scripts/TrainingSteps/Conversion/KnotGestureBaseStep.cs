using System;
using NMY.GoogleCloudTextToSpeech;
using NMY.VirtualRealityTraining.Steps;
using NMY.VirtualRealityTraining.VirtualAssistant;
using UnityEngine;
using UnityEngine.Localization;

namespace DFKI.NMY
{

    using Cysharp.Threading.Tasks;
    using System.Threading;

    public class KnotGestureBaseStep : BaseTrainingStep
    {
        private bool _finishedCriteria = false;

        [Header("Step Data")] [SerializeField] private LocalizedString stepTitle;
        [SerializeField] private LocalizedString stepDescription;
        [SerializeField] private LocalizedTextToSpeechAudioClip ttsContainer;

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

        public LocalizedTextToSpeechAudioClip TtsContainer
        {
            get => ttsContainer;
            set => ttsContainer = value;
        }

        protected override async UniTask ClientStepActionAsync(CancellationToken ct)
        {
            try
            {
                await WaitForFinishedCriteria(ct);
                RaiseClientStepFinished();
            }
            catch (OperationCanceledException)
            {
                RaiseClientStepFinished();
            }

        }

        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            FinishedCriteria = false;
            UserInterfaceManager.instance.UpdateStepInfos(stepTitle, stepDescription);
            
            if (ttsContainer.IsEmpty == false) {
                VirtualAssistant.instance.Speak(ttsContainer, ct);
            }
        }

        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
            VirtualAssistant.instance.StopSpeaking();
        }
        
        public virtual UniTask WaitForFinishedCriteria(CancellationToken ct)
        {
            return UniTask.WaitUntil(() => _finishedCriteria, cancellationToken: ct);
        }

    }
}