using NMY;
using NMY.VirtualRealityTraining.VirtualAssistant;
using UnityEngine;
using UnityEngine.Assertions;

namespace DFKI.NMY
{
    public class GreifbARApp : SingletonStartupBehaviour<GreifbARApp>
    {
        [Header("Various")] 
        public UserInterfaceManager userInterfaceManager;
        public PointCloudPlayer pointCloudPlayer;
        public GestureSequencePlayer gestureSequencePlayer;
        public BVHManager bvhManager;
        public HandVisualizer handVisualizer;
        public VirtualAssistant virtualAssistant;
        [Header("Knotenbank")]
        public SerialController serialController;
        [Header("Varjo")]
        public MarkerManager markerManager;
        public GreifbARVarjoManager varjoManager;

        protected override void StartupEnter()
        {
            base.StartupEnter();
            Assert.IsNotNull(userInterfaceManager);
            Assert.IsNotNull(pointCloudPlayer);
            Assert.IsNotNull(gestureSequencePlayer);
            Assert.IsNotNull(bvhManager);
            Assert.IsNotNull(handVisualizer);
            Assert.IsNotNull(virtualAssistant);
            Assert.IsNotNull(serialController);
            Assert.IsNotNull(markerManager);
            Assert.IsNotNull(varjoManager);
        }


        
        
    }
}
