using System.Collections;
using NMY;
using NMY.VTT.Core;
using UnityEngine;

using Image = UnityEngine.UI.Image;


namespace DFKI.NMY.TrainingSteps
{
    
public class GestureTrainingStep : GestureBaseStep
{

    [Header("Gesture Training Step")] 
    [SerializeField] private int sequenceIndex = 0;
    [SerializeField] private float minDuration = 0.5f;
    [SerializeField] private float maxDuration = 5;
    [SerializeField] private float poseMatchingThreshold = 25;
 
    [SerializeField] private bool loopSequence = true;
    
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
        GestureSequencePlayer.instance.SequenceDuration = minDuration;
        GestureSequencePlayer.instance.PlayAllSequences = false;
        GestureSequencePlayer.instance.LoopSingleSequencePlayback = true;
        GestureSequencePlayer.instance.AnalyzePoseMatching = true;
        GestureSequencePlayer.instance.Play(sequenceIndex);
        /*TODO: Mark Peter --> re Impl 
        GestureSequencePlayer.instance.SequenceDuration = Mathf.Lerp(maxDuration, minDuration, UserInterfaceManager.instance.PlaybackSpeedScrollbar.value);
        UserInterfaceManager.instance.PlaybackSpeedScrollbar.onValueChanged.AddListener((val)=> {
            float newDuration = Mathf.Lerp(maxDuration, minDuration, val);
            GestureSequencePlayer.instance.ChangeDuration(newDuration);
        });
        */
        
        // Register for finish events
        GestureSequencePlayer.instance.SequenceFinishedEvent.AddListener(OnGestureEvent);
        
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

        if (matchedLeft && matchedRight)
        {
            StartCoroutine(TriggerDelayedCompletion());
        }
        
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
        if (Input.GetKeyDown(KeyCode.N)) {
            RaiseStepCompletedEvent();
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
