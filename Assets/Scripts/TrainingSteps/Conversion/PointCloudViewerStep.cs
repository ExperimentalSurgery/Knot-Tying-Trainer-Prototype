using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NMY.VirtualRealityTraining.VirtualAssistant;
using UnityEngine;

namespace DFKI.NMY
{
    
    
    public class PointCloudViewerStep : KnotGestureBaseStep
    {                                                           
        [SerializeField] private string pathToSequence = "PointClouds/***";
        [SerializeField] private PointCloudPlayer player;
        [SerializeField] private PointCloudManager manager;
        
        
        
        protected override async UniTask ClientStepActionAsync(CancellationToken ct)
        {
            try
            {
                await VirtualAssistant.instance.Speak(TtsContainer, ct);
                RaiseClientStepFinished();
            }
            catch (OperationCanceledException)
            {
                RaiseClientStepFinished();
            }

        }
        
        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            VirtualAssistant.instance.StopSpeaking();     
            player.pathToSequence = pathToSequence;
            //player.FinishedPointCloudPlayback.AddListener(OnPlaybackFinished);
            manager.playStream = true;
        }
        
        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct) {
            await base.PostStepActionAsync(ct);
            //player.FinishedPointCloudPlayback.RemoveListener(OnPlaybackFinished);
            manager.playStream = false;    
        }
        
        

        private void OnPlaybackFinished() {
            FinishedCriteria = true;
        }
    }
}
