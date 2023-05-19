using System.Collections;
using System.Collections.Generic;
using NMY.VTT.Core;
using PDollarGestureRecognizer;
using UnityEngine;
using UnityEngine.UIElements;

namespace DFKI.NMY
{
    public class GestureScenarioPreviewStep : GestureBaseStep
    {
        
        [Header("Sequences Config")]
        [SerializeField] private float singleSequenceDuration = 2f;
        
        
        // runtime vars
        private bool finishedLeft = false;
        private bool finishedRight = false;
        protected override void ActivateEnter()
        {
            base.ActivateEnter();
            
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
            HandVisualizer.instance.ResetColor();
            
            // Register for finish events
            GestureSequencePlayer.instance.AllSequencesPlayedEvent.AddListener(OnScenarioPlaybackFinished);
            
            // reset check vars
            finishedLeft = false;
            finishedRight = false;

        }

        private void OnScenarioPlaybackFinished(HandGestureParams eventParams)
        {
            if (eventParams.side.Equals(Hand.Left)) {
                finishedLeft = true;
            }

            if (eventParams.side.Equals(Hand.Right)) {
                finishedRight = true;
            }

            if (finishedLeft && finishedRight)
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
