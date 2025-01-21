using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

namespace DFKI.NMY
{
  
    public enum GestureCheckMethod {Manually=0,PoseMatch=1}

    
    
    public class KnotGestureStep : BVHGestureBaseStep,IKnotbAR
    {

        [Header("CompletionConfig")] 
        [SerializeField] private GestureConfig config;
        [SerializeField] private float successOutlineDuration = 2.5f;

        [SerializeField] private float successCheckmarkDuration = 2.5f;
        [SerializeField] private GestureCheckMethod checkMethod = GestureCheckMethod.PoseMatch;
        [SerializeField] private KeyCode manualCompletionKey = KeyCode.N;
        [Range(0,50)] [SerializeField] private float poseMatchingThresholdRight = 5;
        [Range(0,50)][SerializeField] private float poseMatchingThresholdLeft = 5;
        
        [Header("Finger Configuration - Left")]
        [SerializeField] [Range(0f,1f)] private float thumbLeftWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] private float indexLeftWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] private float middleLeftWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] private float ringLeftWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] private float pinkyLeftWeight = 1.0f;
        
        [Header("Finger Configuration - Right")]
        [SerializeField] [Range(0f,1f)] private float thumbRightWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] private float indexRightWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] private float middleRightWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] private float ringRightWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] private float pinkyRightWeight = 1.0f;

        [Header("Wrist Rotation Weights - Left")]
        [SerializeField] [Range(0f,15f)] private float wristXLeftWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] private float wristYLeftWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] private float wristZLeftWeight = 5.0f;
        [Header("Wrist Rotation Weights - Right")]
        [SerializeField] [Range(0f,15f)] private float wristXRightWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] private float wristYRightWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] private float wristZRightWeight = 5.0f;

        [Header("Elbow Rotation Weights - Left")]
        [SerializeField] [Range(0f,15f)] public float elbowXLeftWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] public float elbowYLeftWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] public float elbowZLeftWeight = 5.0f;
        
        [Header("Elbow Rotation Weights - Right")]
        [SerializeField] [Range(0f,15f)] public float elbowXRightWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] public float elbowYRightWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] public float elbowZRightWeight = 5.0f;


        
        // helper vars
        private bool matchedLeft = false;
        private bool matchedRight = false;
    
        
        public float CumulatedThreshold => (poseMatchingThresholdRight+poseMatchingThresholdLeft)*0.5f;
        
        protected override void Awake()
        {
            
            if (config != null)
            {
                

                thumbLeftWeight = config.thumbLeftWeight;
                indexLeftWeight = config.indexLeftWeight;
                middleLeftWeight = config.middleLeftWeight;
                ringLeftWeight = config.ringLeftWeight;
                pinkyLeftWeight = config.pinkyLeftWeight;

                thumbRightWeight = config.thumbRightWeight;
                indexRightWeight = config.indexRightWeight;
                middleRightWeight = config.middleRightWeight;
                ringRightWeight = config.ringRightWeight;
                pinkyRightWeight = config.pinkyRightWeight;

                wristXLeftWeight = config.wristXLeftWeight;
                wristYLeftWeight = config.wristYLeftWeight;
                wristZLeftWeight = config.wristZLeftWeight;
                
                wristXRightWeight = config.wristXRightWeight;
                wristYRightWeight = config.wristYRightWeight;
                wristZRightWeight = config.wristZRightWeight;

                elbowXLeftWeight = config.elbowXLeftWeight;
                elbowYLeftWeight = config.elbowYLeftWeight;
                elbowZLeftWeight = config.wristZLeftWeight;
                
                elbowXRightWeight = config.elbowXRightWeight;
                elbowYRightWeight = config.elbowYRightWeight;
                elbowZRightWeight = config.elbowZRightWeight;

                poseMatchingThresholdLeft = config.poseMatchingThresholdLeft;
                poseMatchingThresholdRight = config.poseMatchingThresholdRight;

                SequenceIndexLeft = config.sequenceLeft;
                SequenceIndexRight = config.sequenceRight;

                BvhFileLeft = config.bvhPathLeft != "empty" ? config.bvhPathLeft : BvhFileLeft;
                BvhFileRight = config.bvhPathRight != "empty" ? config.bvhPathRight : BvhFileRight;

            }
        }
        
        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
    
            await base.PreStepActionAsync(ct);
            GreifbARApp.instance.SkipGestureTriggered.AddListener(OnSkippedButtonClicked);
            UserInterfaceManager.instance.SetContinueButtonVisible(true);
            
            matchedLeft = false;
            matchedRight = false;
       
            // Apply config to GestureSequencePlayer
            GestureSequencePlayer.instance.PoseMatchingThresholdRight = poseMatchingThresholdRight;
            GestureSequencePlayer.instance.PoseMatchingThresholdLeft = poseMatchingThresholdLeft;
            GestureSequencePlayer.instance.ToggleSpeed(useDefault:true);
            GestureSequencePlayer.instance.PlayAllSequences = false;
            GestureSequencePlayer.instance.LoopSingleSequencePlayback = true;
            GestureSequencePlayer.instance.AnalyzePoseMatching = true;
            
            
            // Set Finger Config
            GestureSequencePlayer.instance.ToggleThumb_Left((int)thumbLeftWeight == 1);
            GestureSequencePlayer.instance.ToggleIndex_Left((int)indexLeftWeight == 1);
            GestureSequencePlayer.instance.ToggleMiddle_Left((int)middleLeftWeight == 1);
            GestureSequencePlayer.instance.ToggleRing_Left((int)ringLeftWeight == 1);
            GestureSequencePlayer.instance.TogglePinky_Left((int)pinkyLeftWeight == 1);

            GestureSequencePlayer.instance.ToggleThumb_Right((int)thumbRightWeight == 1);
            GestureSequencePlayer.instance.ToggleIndex_Right((int)indexRightWeight == 1 );
            GestureSequencePlayer.instance.ToggleMiddle_Right((int)middleRightWeight == 1);
            GestureSequencePlayer.instance.ToggleRing_Right((int)ringRightWeight == 1);
            GestureSequencePlayer.instance.TogglePinky_Right((int)pinkyRightWeight == 1);
            
            GestureSequencePlayer.instance.SetWristWeightsLeft(wristXLeftWeight,wristYLeftWeight,wristZLeftWeight);
            GestureSequencePlayer.instance.SetWristWeightsRight(wristXRightWeight,wristYRightWeight,wristZRightWeight);
            GestureSequencePlayer.instance.SetElbowWeightsLeft(elbowXLeftWeight,elbowYLeftWeight,elbowZLeftWeight);
            GestureSequencePlayer.instance.SetElboWeightsRight(elbowXRightWeight,elbowYRightWeight,elbowZRightWeight);
            
            // START
            GestureSequencePlayer.instance.Play(SequenceIndexLeft,SequenceIndexRight);
            
            HandVisualizer.instance.ResetCheckmarks();
       
            // Register for finish events
            switch (checkMethod)
            {
                case GestureCheckMethod.Manually:
                    break;
                case GestureCheckMethod.PoseMatch:
                    GestureSequencePlayer.instance.GestureCheckChanged.AddListener(OnGestureEvent);
                    break;
            }
        
            StopAllCoroutines();
          
        }

       


        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct) {
            await base.PostStepActionAsync(ct);
            GestureSequencePlayer.instance.GestureCheckChanged.RemoveListener(OnGestureEvent);
            GestureSequencePlayer.instance.Stop();
            UserInterfaceManager.instance.SetContinueButtonVisible(false);
            GreifbARApp.instance.SkipGestureTriggered.RemoveListener(OnSkippedButtonClicked);

            
        }
        
        private void OnSkippedButtonClicked()
        {
            if (stepState == StepState.StepStarted && FinishedCriteria == false)
            {
                FinishedCriteria = true;
            }
        }

        protected override void StepCompletedAction()
        {
            base.StepCompletedAction();
            HandVisualizer.instance.ResetCheckmarks();
        }
        
        private void OnGestureEvent(HandGestureParams parameters) {

            if (parameters.isMatching &&  parameters.passedMinDuration && parameters.side.Equals(Hand.Left) && !matchedLeft) {
                matchedLeft = true;
                HandVisualizer.instance.FlashHandOnceOnSuccess(true,false);
                HandVisualizer.instance.SetSuccessOutline(true,false);
                HandVisualizer.instance.SetSuccessCheckmark(true, false);
            }
            else if (parameters.isMatching && parameters.passedMinDuration &&    parameters.side.Equals(Hand.Right) && !matchedRight) {
                matchedRight = true;
                HandVisualizer.instance.FlashHandOnceOnSuccess(false,true);
                HandVisualizer.instance.SetSuccessOutline(false,true);
                HandVisualizer.instance.SetSuccessCheckmark(false, true);
            }
            
            

        
        }
        
        
        private void Update()
        {
            if (stepState.Equals(StepState.StepStarted) &&  checkMethod.Equals(GestureCheckMethod.Manually) && Input.GetKeyDown(manualCompletionKey)) {
                TriggerCompletion();
            }
            else if(stepState.Equals(StepState.StepStarted))
            {
                if (matchedLeft && matchedRight) {
                    TriggerCompletion();
                }
            }
            
        }
        
        public void TriggerCompletion() {
        
            GestureSequencePlayer.instance.GestureCheckChanged.RemoveListener(OnGestureEvent);
            GestureSequencePlayer.instance.Stop();
            //HandVisualizer.instance.SetTimedSuccessOutline(successOutlineDuration,true,true);
            //HandVisualizer.instance.SetTimedSuccessCheckmarks(successCheckmarkDuration,true,true);
            FinishedCriteria = true;
        }
        
    }
}
