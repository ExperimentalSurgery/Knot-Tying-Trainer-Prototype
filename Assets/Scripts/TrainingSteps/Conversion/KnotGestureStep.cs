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
        [SerializeField] private GestureCheckMethod checkMethod = GestureCheckMethod.PoseMatch;
        [SerializeField] private KeyCode manualCompletionKey = KeyCode.N;
        [SerializeField] private float sequenceDuration = 0.5f;
        [SerializeField] private float poseMatchingThreshold = 25;
        

        // helper vars
        private bool matchedLeft = false;
        private bool matchedRight = false;
        
        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct) {
            await base.PreStepActionAsync(ct);
            matchedLeft = false;
            matchedRight = false;
            
            // Apply config to GestureSequencePlayer
            GestureSequencePlayer.instance.PoseMatchingThreshold = poseMatchingThreshold;
            GestureSequencePlayer.instance.ToggleSpeed(useDefault:true);
            GestureSequencePlayer.instance.ChangeDuration(sequenceDuration);
            GestureSequencePlayer.instance.PlayAllSequences = false;
            GestureSequencePlayer.instance.LoopSingleSequencePlayback = true;
            GestureSequencePlayer.instance.AnalyzePoseMatching = true;
            GestureSequencePlayer.instance.Play(SequenceIndex);
       
       
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
                HandVisualizer.instance.SetSuccessColor(true,false);
            }
            else if (parameters.isMatching && parameters.side.Equals(Hand.Right)) {
                Debug.Log("Matched right");
                matchedRight = true;
                HandVisualizer.instance.SetSuccessColor(false,true);
            }

            if (matchedLeft && matchedRight) {
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
            HandVisualizer.instance.SetExpertHandVisibleRight(false);
            HandVisualizer.instance.SetExpertHandVisibleLeft(false);
            FinishedCriteria = true;
        }
        
    }
}
