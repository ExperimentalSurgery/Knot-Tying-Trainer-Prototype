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
        
        [SerializeField] private string bvhFileLeft;
        [SerializeField] private string bvhFileRight;
        [FormerlySerializedAs("sequenceIndex")] [SerializeField] private int sequenceIndexLeft = 0;
        [SerializeField] private int sequenceIndexRight = 0;
        
        public string BvhFileLeft
        {
            get => bvhFileLeft;
            set => bvhFileLeft = value;
        }

        public string BvhFileRight
        {
            get => bvhFileRight;
            set => bvhFileRight = value;
        }

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
   
            GestureSequencePlayer.instance.LeftHandBvhFile = bvhFileLeft;
            GestureSequencePlayer.instance.RightHandBvhFile = bvhFileRight;
            GestureSequencePlayer.instance.InitSequence();
        
            
            
        }
        
        

        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct) {
            await base.PostStepActionAsync(ct);
        }

    }
}
