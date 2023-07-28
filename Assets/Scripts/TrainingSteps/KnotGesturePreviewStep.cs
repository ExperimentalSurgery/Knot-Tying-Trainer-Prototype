using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using NMY.VirtualRealityTraining.VirtualAssistant;
using System.Threading.Tasks;

namespace DFKI.NMY
{
    public class KnotGesturePreviewStep : BVHGestureBaseStep
    {

        [Header("KnotGesturePreviewStep")] 
        [SerializeField] private float singleSequenceDuration = 2f;

        public bool finishStepOnSeqEnd = true;

        [Tooltip("[Optional] Instead of using the currennt config of the GestureSequencePlayer the file-references will be set and the player re-initialiuzed")]
        // runtime vars
        private bool finishedLeft = false;
        private bool finishedRight = false;

        public int extraDelay;
        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);

            FinishedCriteria = false;
            
            // Apply config to gestureplayer
            GestureSequencePlayer.instance.SequenceDuration = singleSequenceDuration;
            GestureSequencePlayer.instance.PlayAllSequences = true;
            GestureSequencePlayer.instance.LoopAllSequences = true;
            GestureSequencePlayer.instance.AnalyzePoseMatching = false;
            GestureSequencePlayer.instance.Play();
            
            // show expert hands
            HandVisualizer.instance.SetExpertHandVisibleRight(true);
            HandVisualizer.instance.SetExpertHandVisibleLeft(true);
        
            // Reset colors of user hands
            HandVisualizer.instance.ResetOutline();
            
            // Register for finish events
            GestureSequencePlayer.instance.AllSequencesPlayedEvent.AddListener(OnScenarioPlaybackFinished);
            
            // reset check vars
            finishedLeft = false;
            finishedRight = false;

        }

        private void OnScenarioPlaybackFinished(HandGestureParams eventParams)
        {
            if(!finishStepOnSeqEnd)
                return;

            if (eventParams.side.Equals(Hand.Left)) {
                finishedLeft = true;
            }

            if (eventParams.side.Equals(Hand.Right)) {
                finishedRight = true;
            }

            if (finishedLeft && finishedRight) {
                FinishedCriteria = true;
            }
        }

        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            if(extraDelay > 0)
                await Task.Delay(extraDelay);  

            await base.PostStepActionAsync(ct);
            SFXManager.instance.StopAudio();
            GestureSequencePlayer.instance.Stop();
        }
    }
}