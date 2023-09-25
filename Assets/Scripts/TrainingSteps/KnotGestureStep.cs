using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

namespace DFKI.NMY
{
  
    public enum GestureCheckMethod {Manually=0,PoseMatch=1}

    
    public class KnotGestureStep : BVHGestureBaseStep
    {


        [Header("CompletionConfig")] 
        [SerializeField] private float successOutlineDuration = 2.5f;
        [SerializeField] private GestureCheckMethod checkMethod = GestureCheckMethod.PoseMatch;
        [SerializeField] private KeyCode manualCompletionKey = KeyCode.N;
        [SerializeField] private float poseMatchingThreshold = 25;
        [SerializeField] private bool requireLeftMatch = true;
        [SerializeField] private bool requireRightMatch = true;
       
        // helper vars
        private bool matchedLeft = false;
        private bool matchedRight = false;
        
        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct) {
            await base.PreStepActionAsync(ct);
            matchedLeft = false;
            matchedRight = false;
            
            // Apply config to GestureSequencePlayer
            GestureSequencePlayer.instance.PoseMatchingThresholdRight = poseMatchingThreshold;
            GestureSequencePlayer.instance.ToggleSpeed(useDefault:true);
            GestureSequencePlayer.instance.PlayAllSequences = false;
            GestureSequencePlayer.instance.LoopSingleSequencePlayback = true;
            GestureSequencePlayer.instance.AnalyzePoseMatching = true;
            GestureSequencePlayer.instance.Play(SequenceIndexLeft,SequenceIndexRight);
       
       
            // Register for finish events
            switch (checkMethod)
            {
                case GestureCheckMethod.Manually:
                    break;
                case GestureCheckMethod.PoseMatch:
                    GestureSequencePlayer.instance.SequenceFinishedEvent.AddListener(OnGestureEvent);
                    break;
            }
        
            StopAllCoroutines();
          
        }
        
        
        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct) {
            await base.PostStepActionAsync(ct);
            GestureSequencePlayer.instance.SequenceFinishedEvent.RemoveListener(OnGestureEvent);
            GestureSequencePlayer.instance.Stop();
            
        }
        
        private void OnGestureEvent(HandGestureParams parameters) {

            if (parameters.isMatching && parameters.side.Equals(Hand.Left)) {
                Debug.Log("Matched left");
                matchedLeft = true;
                HandVisualizer.instance.SetSuccessOutline(true,false);
            }
            else if (parameters.isMatching && parameters.side.Equals(Hand.Right)) {
                Debug.Log("Matched right");
                matchedRight = true;
                HandVisualizer.instance.SetSuccessOutline(false,true);
            }
            
            

            if ((matchedLeft || !requireLeftMatch) && (matchedRight || !requireRightMatch)) {
                TriggerCompletion();
            }
        
        }
        
        
        private void Update()
        {
            if (stepState.Equals(StepState.StepStarted) &&  checkMethod.Equals(GestureCheckMethod.Manually) && Input.GetKeyDown(manualCompletionKey)) {
                TriggerCompletion();
            }
        }
        
        public void TriggerCompletion() {
        
            GestureSequencePlayer.instance.SequenceFinishedEvent.RemoveListener(OnGestureEvent);
            GestureSequencePlayer.instance.Stop();
            HandVisualizer.instance.SetTimedSuccessOutline(successOutlineDuration,true,true);
            FinishedCriteria = true;
        }
        
    }
}
