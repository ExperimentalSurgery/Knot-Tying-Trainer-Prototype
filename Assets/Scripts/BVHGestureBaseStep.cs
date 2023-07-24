using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DFKI.NMY
{
    public class BVHGestureBaseStep : GreifbarBaseStep {
        
        [Header("Gesture Training Step")] 
        [Tooltip("[Optional] Instead of using the currennt config of the GestureSequencePlayer the file-references will be set and the player re-initialiuzed")]
        
        [SerializeField] private bool changeBvhFile=false;
        [SerializeField] private string bvhFileLeft;
        [SerializeField] private string bvhFileRight;
        [SerializeField] private int sequenceIndex = 0;
        [SerializeField] private bool showExpertHands = true;
        [SerializeField] private bool showProgressBar = true;
        public int SequenceIndex
        {
            get => sequenceIndex;
            set => sequenceIndex = value;
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
            if(showProgressBar)UserInterfaceManager.instance.ShowProgressIndicator();
            else UserInterfaceManager.instance.HideProgressIndicator();
            
            
        }
        
        

        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct) {
            await base.PostStepActionAsync(ct);
            ExpertHands.instance.gameObject.SetActive(false);
            UserInterfaceManager.instance.HideProgressIndicator();
        }

    }
}
