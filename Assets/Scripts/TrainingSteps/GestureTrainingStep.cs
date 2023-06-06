using System.Collections;
using Microsoft.MixedReality.Toolkit.UI;
using NMY;
using NMY.VTT.Core;
using UnityEngine;

using Image = UnityEngine.UI.Image;


namespace DFKI.NMY.TrainingSteps
{
 
    public enum GestureCheckMethod {Manually=0,PoseMatch=1}
public class GestureTrainingStep : GestureBaseStep
{
    
    [Header("Gesture Training Step")] 
    [SerializeField] private int sequenceIndex = 0;

    [Header("CompletionConfig")]
    [SerializeField] private GestureCheckMethod checkMethod = GestureCheckMethod.PoseMatch;
    [SerializeField] private KeyCode manualCompletionKey = KeyCode.N;
    [SerializeField] private float sequenceDuration = 0.5f;
    [SerializeField] private float poseMatchingThreshold = 25;

    // helper vars
    private bool matchedLeft = false;
    private bool matchedRight = false;
    
    protected override void ActivateEnter()
    {
        base.ActivateEnter();
        matchedLeft = false;
        matchedRight = false;
        
        // Apply config to GesturePlayer
        GestureSequencePlayer.instance.PoseMatchingThreshold = poseMatchingThreshold;
        GestureSequencePlayer.instance.ToggleSpeed(useDefault:true);
        GestureSequencePlayer.instance.ChangeDuration(sequenceDuration);
        GestureSequencePlayer.instance.PlayAllSequences = false;
        GestureSequencePlayer.instance.LoopSingleSequencePlayback = true;
        GestureSequencePlayer.instance.AnalyzePoseMatching = true;
        GestureSequencePlayer.instance.Play(sequenceIndex);
       
       
        // Register for finish events
        switch (checkMethod)
        {
            case GestureCheckMethod.Manually:
                break;
            case GestureCheckMethod.PoseMatch:
                GestureSequencePlayer.instance.SequenceFinishedEvent.AddListener(OnGestureEvent);
                break;
        }
        
        
        // show expert hands
        HandVisualizer.instance.SetExpertHandVisibleRight(true);
        HandVisualizer.instance.SetExpertHandVisibleLeft(true);
        
        // Reset colors of user hands
        HandVisualizer.instance.ResetColor();
        
        StopAllCoroutines();

    }


    private void OnGestureEvent(HandGestureParams parameters) {

        if (parameters.isMatching && parameters.side.Equals(Hand.Left)) {
            Debug.Log("Matched left");
            matchedLeft = true;
            HandVisualizer.instance.SetSuccessColor(true,false);
        }
        else if (parameters.isMatching && parameters.side.Equals(Hand.Right)) {
            Debug.Log("Matched right");
            matchedRight = true;
            HandVisualizer.instance.SetSuccessColor(false,true);
        }

        if (matchedLeft && matchedRight) {
           TriggerCompletionManually();
        }
        
    }

    public void TriggerCompletionManually() {
        
        StartCoroutine(TriggerDelayedCompletion());
    }

    public override IEnumerator TriggerDelayedCompletion() {
        GestureSequencePlayer.instance.SequenceFinishedEvent.RemoveListener(OnGestureEvent);
        GestureSequencePlayer.instance.Stop();
        HandVisualizer.instance.SetExpertHandVisibleRight(false);
        HandVisualizer.instance.SetExpertHandVisibleLeft(false);    
        UserInterfaceManager.instance.ShowSuccessPanel();
        return base.TriggerDelayedCompletion();
    }

    protected override void DeactivateEnter()
    {
        base.DeactivateEnter();
        GestureSequencePlayer.instance.SequenceFinishedEvent.RemoveListener(OnGestureEvent);
        UserInterfaceManager.instance.HideSuccessPanel();
        GestureSequencePlayer.instance.Stop();
    }

    private void Update()
    {
        if (isActivated && !locked && checkMethod.Equals(GestureCheckMethod.Manually) && Input.GetKeyDown(manualCompletionKey)) {
            TriggerCompletionManually();
        }
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
