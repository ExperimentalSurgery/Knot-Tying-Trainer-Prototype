using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DFKI.NMY.PoincloudPlayer;
using NMY.VirtualRealityTraining.VirtualAssistant;
using UnityEngine;

namespace DFKI.NMY
{
    

    public class PointCloudViewerStep : GreifbarBaseStep
    {
        
        [Header("PointCloudViewerStep")]
        public bool manipulatePlayerPose = false;
        public Vector3 playerPosition = Vector3.zero;
        public Vector3 playerRotation = Vector3.zero;
        [Header("PointCloud Source Config")]
        [SerializeField] private string pathToSequence = "PointClouds/***";
        [SerializeField] private PointCloudPlayer player;

        [Header("(Optional) Player Settings")] [SerializeField]
        private bool manipulateFPS = false;
        [SerializeField] private int targetFPS = 30;
        

        protected override void Reset()
        {
            base.Reset();
            player = FindObjectOfType<PointCloudPlayer>(true);
            this.name =  "[PointCloudViewerStep] " + this.name.Trim(' ');
        }

        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);

            if (player == null) {
                Debug.LogError("PointcloudPlayer not referenced in go="+gameObject.name);
                return;
            }

            if (manipulatePlayerPose) {
                player.playerPosition = playerPosition;
                player.playerRotation = playerRotation;
            }

            // stop current player actions
            player.StopThread();
            
            // Setup new stream
            player.PathToSequence = pathToSequence;
            player.SetupReaderAndPCManager();
            player.FPS = manipulateFPS ? targetFPS : player.FPS;
            player.Play();

        }


        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct) {
            await base.PostStepActionAsync(ct);
            player.StopThread();
        }
        
    }
}
