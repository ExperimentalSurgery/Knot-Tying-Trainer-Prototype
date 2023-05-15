using System.Collections;
using System.Collections.Generic;
using NMY.VTT.Core;
using UnityEngine;


namespace DFKI.NMY.TrainingSteps
{
    
public class GestureTrainingStep : VTTBaseListStep
{

    [Header("Gesture Training Step")] 
    [SerializeField] private int sequenceIndex = 0;
    [SerializeField] private float playDuration = 1;
    
    protected override void ActivateEnter()
    {
        base.ActivateEnter();
        GestureSequencePlayer.instance.sequenceDuration = playDuration;
        GestureSequencePlayer.instance.Play(sequenceIndex);
    }

    protected override void DeactivateEnter()
    {
        base.DeactivateEnter();
        GestureSequencePlayer.instance.Stop();
    }

    protected override void DeactivateImmediatelyEnter() => DeactivateEnter();
    protected override void ActivateImmediatelyEnter() => ActivateEnter();
    
    protected override void OnStepComplete()
    {
        
    }
    protected override void OnStepReset()
    {
    }

    protected override void OnPause()
    {
    }

    protected override void OnResume()
    {
    }
}
}
