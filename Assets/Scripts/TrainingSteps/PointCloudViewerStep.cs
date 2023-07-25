using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NMY.VirtualRealityTraining.VirtualAssistant;
using UnityEngine;

namespace DFKI.NMY
{
    
    
    public class PointCloudViewerStep : GreifbarBaseStep
    {                                                           
        [Header("PointCloudViewerStep")]
        [SerializeField] private string pathToSequence = "PointClouds/***";
        [SerializeField] private PointCloudPlayer player;
        [SerializeField] private PointCloudManager manager;

        [Header("(Optional) Player Settings")] [SerializeField]
        private bool manipulateFPS = false;
        [SerializeField] private int targetFPS = 30;

        
        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            manager.fps = manipulateFPS ? targetFPS : manager.fps;
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
        
        /*
         private void OnPlaybackFinished() {
            FinishedCriteria = true;
        }
        */
    }
}
