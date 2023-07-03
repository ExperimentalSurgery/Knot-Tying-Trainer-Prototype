using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DFKI.NMY;
using NMY.GoogleCloudTextToSpeech;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;
using UnityEngine.Localization;

public class KnotGestureBaseStep : BaseTrainingStep
{
    private bool _finishedCriteria = false;

    [Header("Step Data")] [SerializeField] private LocalizedString stepTitle;
    [SerializeField] private LocalizedString stepDescription;
    [SerializeField] private LocalizedTextToSpeechItem spokenTextTts;
    
    public LocalizedString StepTitle {
        get => stepTitle;
        set => stepTitle = value;
    }

    public LocalizedString StepDescription {
        get => stepDescription;
        set => stepDescription = value;
    }

    protected override async UniTask ClientStepActionAsync(CancellationToken ct) {
        try {
            await WaitForFinishedCriteria(ct);
            RaiseClientStepFinished();
        }
        catch (OperationCanceledException) {
            RaiseClientStepFinished();
        }

    }

    // PRE STEP
    protected override async UniTask PreStepActionAsync(CancellationToken ct) {
        await base.PreStepActionAsync(ct);
        UserInterfaceManager.instance.UpdateStepInfos(stepTitle, stepDescription);

        if (spokenTextTts && spokenTextTts.audioClip)
        {
            SFXManager.instance.PlayAudio(spokenTextTts.audioClip);
        }
    }

    // POST STEP
    protected override async UniTask PostStepActionAsync(CancellationToken ct) {
        await base.PostStepActionAsync(ct);
        SFXManager.instance.StopAudio();
    }

    public virtual UniTask WaitForFinishedCriteria(CancellationToken ct) {
        return UniTask.WaitUntil(() => _finishedCriteria, cancellationToken: ct);
    }
    
}
