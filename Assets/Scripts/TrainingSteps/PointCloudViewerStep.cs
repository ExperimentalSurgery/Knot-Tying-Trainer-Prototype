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


        protected override void Reset()
        {
            base.Reset();
            player = FindObjectOfType<PointCloudPlayer>(true);
            manager = FindObjectOfType<PointCloudManager>(true);
            this.name =  "[PointCloudViewerStep] " + this.name.Trim(' ');
        }

        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            
            // stop current player actions
            player.gameObject.SetActive(true);
            player.StopThread();
            manager.playStream = false;
            
            // Setup new stream
            player.pathToSequence = pathToSequence;
            player.SetupReaderAndPCManager();
            manager.fps = manipulateFPS ? targetFPS : manager.fps;
            manager.playStream = true;
        }
        
        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct) {
            await base.PostStepActionAsync(ct);
            player.StopThread();
            manager.playStream = false;
            player.gameObject.SetActive(false);
        }
        
    }
}
