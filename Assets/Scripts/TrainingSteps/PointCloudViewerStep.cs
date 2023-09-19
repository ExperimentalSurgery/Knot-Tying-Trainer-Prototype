using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace DFKI.NMY
{
    
    public class PointCloudViewerStep : GreifbarBaseStep
    {
        
        [Header("PointCloudViewerStep")]
        public bool waitForStreamPlayedOnce = true;
        public bool manipulatePlayerPose = false;
        public Vector3 playerPosition;
        public Vector3 playerRotation;
        [Header("PointCloud Source Config")]
        [SerializeField] private string pathToSequence = "PointClouds/***";
        

        [Header("(Optional) Player Settings")] [SerializeField]
        private bool manipulateFPS = false;
        [SerializeField] private int targetFPS = 30;

        private GreifbarGeometrySequencePlayer player;

        protected override void Reset()
        {
            base.Reset();
            this.name =  "[PointCloudViewerStep] " + this.name.Trim(' ');
            
        }
        void OnStreamFinished(){
            if(waitForStreamPlayedOnce){
                FinishedCriteria = true;
            }

        }
        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            if (!player)
            {
                player = FindObjectOfType<GreifbarGeometrySequencePlayer>(true);
                Assert.IsNotNull(player, "Missing GreifbarGeometryPlayer in scene");
            }

            player.FinishedStream.AddListener(OnStreamFinished);

            if (player == null) {
                Debug.LogError("PointcloudPlayer not referenced in go="+gameObject.name);
                return;
            }

            if (manipulatePlayerPose) {
                player.playerPosition = playerPosition;
                player.playerRotation = playerRotation;
            }

            // stop current player actions
            player.Stop();
            
            // Setup new stream
            player.RelativePath = pathToSequence;
            player.PlaybackFPS = manipulateFPS ? targetFPS : player.PlaybackFPS;
            player.Play();

        }


        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct) {
            await base.PostStepActionAsync(ct);
            player.FinishedStream.RemoveListener(OnStreamFinished);
            player.Stop();
        }
        
    }
}
