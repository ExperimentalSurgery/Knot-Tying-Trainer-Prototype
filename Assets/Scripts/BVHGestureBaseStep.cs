using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace DFKI.NMY
{
    public class BVHGestureBaseStep : GreifbarBaseStep {
        
        [Header("Gesture Training Step")] 
        [Tooltip("[Optional] Instead of using the currennt config of the GestureSequencePlayer the file-references will be set and the player re-initialiuzed")]
        
        [SerializeField] private bool changeBvhFile=false;
        [SerializeField] private string bvhFileLeft;
        [SerializeField] private string bvhFileRight;
        [FormerlySerializedAs("sequenceIndex")] [SerializeField] private int sequenceIndexLeft = 0;
        [SerializeField] private int sequenceIndexRight = 0;
        [SerializeField] private bool showExpertHands = false;

        public int SequenceIndexLeft
        {
            get => sequenceIndexLeft;
            set => sequenceIndexLeft = value;
        }

        public int SequenceIndexRight
        {
            get => sequenceIndexRight;
            set => sequenceIndexRight = value;
        }

        protected override async UniTask PreStepActionAsync(CancellationToken ct) {
            
            await base.PreStepActionAsync(ct);
            // Apply config to GesturePlayer
            if (changeBvhFile) {
                GestureSequencePlayer.instance.LeftHandBvhFile = bvhFileLeft;
                GestureSequencePlayer.instance.RightHandBvhFile = bvhFileRight;
                GestureSequencePlayer.instance.InitSequence();
            }
            
            // For this step we want to activate the Expert Hands as Preview (optionally)
            ExpertHands.instance.gameObject.SetActive(showExpertHands);
            
            
        }
        
        

        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct) {
            await base.PostStepActionAsync(ct);
            ExpertHands.instance.gameObject.SetActive(false);
        }

    }
}
