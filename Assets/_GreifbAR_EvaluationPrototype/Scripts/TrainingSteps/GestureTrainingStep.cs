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
    
    // helper vars
    private bool matchedLeft = false;
    private bool matchedRight = false;
    
    protected override void ActivateEnter()
    {
        base.ActivateEnter();
        matchedLeft = false;
        matchedRight = false;
        GestureSequencePlayer.instance.poseMatchingThreshold = poseMatchingThreshold;
        GestureSequencePlayer.instance.sequenceDuration = minDuration;
        GestureSequencePlayer.instance.Play(sequenceIndex);
        GestureSequencePlayer.instance.sequenceDuration = Mathf.Lerp(maxDuration, minDuration, GestureTrainingUI.instance.PlaybackSpeedScrollbar.value);
        GestureTrainingUI.instance.PlaybackSpeedScrollbar.onValueChanged.AddListener((val)=> {
            float newDuration = Mathf.Lerp(maxDuration, minDuration, val);
            GestureSequencePlayer.instance.ChangeDuration(newDuration);
        });
        
        // Register for finish events
        GestureSequencePlayer.instance.SequenceFinishedEvent.AddListener(OnGestureEvent);
        
        // show expert hands
        HandVisualizer.instance.SetExpertHandVisibleRight(true);
        HandVisualizer.instance.SetExpertHandVisibleLeft(true);

    }

    private void OnGestureEvent(HandGestureParams parameters) {

        if (parameters.isMatching && parameters.leftHand) {
            matchedLeft = true;
            HandVisualizer.instance.SetExpertHandVisibleLeft(false);
        }
        else if (parameters.isMatching && parameters.leftHand == false)
        {
            matchedRight = true;
            HandVisualizer.instance.SetExpertHandVisibleRight(false);
        }

        if (matchedLeft && matchedRight)
        {
            RaiseStepCompletedEvent();
        }
        
    }

    protected override void DeactivateEnter()
    {
        base.DeactivateEnter();
        GestureSequencePlayer.instance.Stop();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
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
