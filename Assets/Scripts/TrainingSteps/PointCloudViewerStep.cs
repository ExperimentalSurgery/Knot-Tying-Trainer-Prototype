using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace DFKI.NMY
{
    
    public class PointCloudViewerStep : GreifbarBaseStep,IKnotbAR
    {

        [SerializeField] private bool allowPreview = true;
        
        [Header("(Optional) Config")] 
        [SerializeField] private GestureConfig config;
        
        [Header("PointCloudViewerStep")] 
        [SerializeField] private string gestureKey = "empty";
        public bool finishWhenStreamPlayedOnce = false;
        [Header("PointCloud Source Config")]
        [SerializeField] private string pathToSequence = "PointClouds/***";
        

        [Header("(Optional) Player Settings")] 
        [SerializeField]
        private bool manipulateFPS = false;
        [SerializeField] private int targetFPS = 30;

        private GreifbarGeometrySequencePlayer player;


        [ContextMenu("Has Preview?")]
        public void DebugHasGesturePreview()
        {
            if (config)
            {
                Debug.Log("gesture available for "+config.gestureKey +" "+GreifbARApp.instance.gesturePreviewManager.PreviewAvailable(config.gestureKey));
            }
            else
            {
                Debug.Log("No config referenced ... no check");
            }
        }
        
        protected override void Reset()
        {
            base.Reset();
            this.name =  "[PointCloudViewerStep] " + this.name.Trim(' ');
            
        }
        void OnStreamFinished(){
            if(finishWhenStreamPlayedOnce){
                FinishedCriteria = true;
            }

        }
        protected override async UniTask PreStepActionAsync(CancellationToken ct) {
            await base.PreStepActionAsync(ct);

            if (config) {
                gestureKey = config.gestureKey != "empty" ? config.gestureKey : gestureKey;
                pathToSequence = config.pointCloudFilePath != "empty" ? config.pointCloudFilePath : pathToSequence;
                Illustration = config.preview != null ? config.preview : Illustration;
                manipulateFPS = config.manipulateFPS;
                targetFPS = config.targetFPS;
            }
            
            if (!player) {
                player = FindObjectOfType<GreifbarGeometrySequencePlayer>(true);
                Assert.IsNotNull(player, "Missing GreifbarGeometryPlayer in scene");
            }

            player.FinishedStream.AddListener(OnStreamFinished);

            if (player == null) {
                Debug.LogError("PointcloudPlayer not referenced in go="+gameObject.name);
                return;
            }
            
            // stop current player actions
            player.Stop();

            // Setup new stream
            float playbackFPS = manipulateFPS ? targetFPS : player.GetTargetFPS() ;
            player.LoadSequence(pathToSequence, player.pathRelation, playbackFPS);

        }

        protected override async UniTask ClientStepActionAsync(CancellationToken ct)
        {
            MeshGesturePreviewManager previewManager = GreifbARApp.instance.gesturePreviewManager;
            if (previewManager && allowPreview) {
                previewManager.UpdateCurrentGesture(gestureKey);
                
            }

            if (previewManager)
            {
                previewManager.UpdateArrowPreview(gestureKey);
                previewManager.ShowArrowPreview();
            }
            await base.ClientStepActionAsync(ct);
        }


        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct) {
            await base.PostStepActionAsync(ct);
            player.FinishedStream.RemoveListener(OnStreamFinished);
            player.Stop();
            
            MeshGesturePreviewManager previewManager = GreifbARApp.instance.gesturePreviewManager;
            if (previewManager) {
                previewManager.HideGesturePreview();
                previewManager.HideArrowPreview();
            }
        }
        
    }
}
