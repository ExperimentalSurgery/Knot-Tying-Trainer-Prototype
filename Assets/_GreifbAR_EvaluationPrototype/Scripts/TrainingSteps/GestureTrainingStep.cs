using System.Collections;
using NMY;
using NMY.VTT.Core;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;


namespace DFKI.NMY.TrainingSteps
{
    
public class GestureTrainingStep : VTTBaseListStep
{

    [Header("Gesture Training Step")] 
    [SerializeField] private int sequenceIndex = 0;
    [SerializeField] private float minDuration = 0.5f;
    [SerializeField] private float maxDuration = 5;
    [SerializeField] private float poseMatchingThreshold = 25;
    [SerializeField] private float nextStepDelay = 2.0f;
    
    // helper vars
    private bool matchedLeft = false;
    private bool matchedRight = false;
    
    protected override void ActivateEnter()
    {
        base.ActivateEnter();
        matchedLeft = false;
        matchedRight = false;
        
        // Apply config to gestureplayer
        GestureSequencePlayer.instance.PoseMatchingThreshold = poseMatchingThreshold;
        GestureSequencePlayer.instance.SequenceDuration = minDuration;
        GestureSequencePlayer.instance.PlayAllSequences = false;
        GestureSequencePlayer.instance.AnalyzePoseMatching = true;
        GestureSequencePlayer.instance.Play(sequenceIndex);
        GestureSequencePlayer.instance.SequenceDuration = Mathf.Lerp(maxDuration, minDuration, GestureTrainingUI.instance.PlaybackSpeedScrollbar.value);
        GestureTrainingUI.instance.PlaybackSpeedScrollbar.onValueChanged.AddListener((val)=> {
            float newDuration = Mathf.Lerp(maxDuration, minDuration, val);
            GestureSequencePlayer.instance.ChangeDuration(newDuration);
        });
        
        // Register for finish events
        GestureSequencePlayer.instance.SequenceFinishedEvent.AddListener(OnGestureEvent);
        
        // show expert hands
        HandVisualizer.instance.SetExpertHandVisibleRight(true);
        HandVisualizer.instance.SetExpertHandVisibleLeft(true);
        
        // Reset colors of user hands
        HandVisualizer.instance.ResetColor();

    }

    private void OnGestureEvent(HandGestureParams parameters) {

        if (parameters.isMatching && parameters.leftHand) {
            Debug.Log("Matched left");
            matchedLeft = true;
            HandVisualizer.instance.SetSuccessColor(true,false);
        }
        else if (parameters.isMatching && parameters.leftHand == false)
        {
            Debug.Log("Matched right");
            matchedRight = true;
            HandVisualizer.instance.SetSuccessColor(false,true);
        }

        if (matchedLeft && matchedRight)
        {
            StartCoroutine(TriggerDelayedCompletion());
        }
        
    }


    public IEnumerator TriggerDelayedCompletion() {
        GestureSequencePlayer.instance.Stop();
        HandVisualizer.instance.SetExpertHandVisibleRight(false);
        HandVisualizer.instance.SetExpertHandVisibleLeft(false);    
        UserInterfaceManager.instance.ShowSuccessPanel();
        yield return new WaitForSeconds(nextStepDelay);
        RaiseStepCompletedEvent();
    }

    protected override void DeactivateEnter()
    {
        base.DeactivateEnter();
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
