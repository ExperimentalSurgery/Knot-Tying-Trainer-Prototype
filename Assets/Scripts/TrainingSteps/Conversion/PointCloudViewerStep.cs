using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DFKI.NMY
{
    
    
    public class PointCloudViewerStep : KnotGestureBaseStep
    {
        [SerializeField] private PointCloudPlayer player;

        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            player.FinishedPointCloudPlayback.AddListener(OnPlaybackFinished);
           
        }
        
        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct) {
            await base.PostStepActionAsync(ct);
            player.FinishedPointCloudPlayback.RemoveListener(OnPlaybackFinished);
        }
        

        private void OnPlaybackFinished() {
            FinishedCriteria = true;
        }
    }
}
