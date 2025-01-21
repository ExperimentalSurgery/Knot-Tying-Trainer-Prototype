using System;
using NMY;
using NMY.VirtualRealityTraining.Steps;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace DFKI.NMY
{
    public class GreifbARApp : SingletonStartupBehaviour<GreifbARApp>
    {

        [Header("Various")] 
        public LevelChoicePanel levelChoice;
        public UserInterfaceManager userInterfaceManager;
        public GreifbarGeometrySequencePlayer pointCloudPlayer;
        public GestureSequencePlayer gestureSequencePlayer;
        public MeshGesturePreviewManager gesturePreviewManager;
        public HandVisualizer handVisualizer;
        public AudioSource mainAudioSource;

        [Header("Knotenbank")]
        public SerialController serialController;
        [Header("Varjo")]
        public GreifbARMarkerManager markerManager;
        public GreifbARVarjoManager varjoManager;
        
        [Header("Keycodes")] 
        public KeyCode backToLevelChoiceKey = KeyCode.Escape;
        
        [Header("Events")]
        public UnityEvent SkipGestureTriggered = new UnityEvent();

        public bool disabledNullChecks = false;
        
        protected override void StartupEnter()
        {
            base.StartupEnter();
            if(disabledNullChecks==false){
            Assert.IsNotNull(userInterfaceManager);
            Assert.IsNotNull(pointCloudPlayer);
            Assert.IsNotNull(gestureSequencePlayer);
            Assert.IsNotNull(handVisualizer);
            Assert.IsNotNull(serialController);
            Assert.IsNotNull(markerManager);
            Assert.IsNotNull(varjoManager);
            Assert.IsNotNull(gesturePreviewManager);
          
            }
            if(levelChoice){
            levelChoice.levelSwitchTriggered.AddListener((level) => {
                switch (level) {
                    case 0:
                        StartIntro();
                        break;
                    case 1:
                        StartLevel1();
                        break;
                    case 2: 
                        StartLevel2();
                        break;
                    case 3:
                        StartLevel3();
                        break;
                }

            });
            }
        }

        private void Update() {

            if (Input.GetKeyDown(backToLevelChoiceKey)) {
                StartLevelChoice();
            }
        }

        /// <summary>
        /// Loads the specified scene in single mode on the server.
        /// </summary>
        /// <remarks>
        /// It propagates the scene change to all clients through <see cref="NetworkSceneManager.LoadScene"/>
        /// </remarks>
        /// <param name="sceneName">The name of the scene to be loaded.</param>
        public static void LoadSceneSingle(string sceneName) {
            if (NetworkManager.Singleton.IsServer && !string.IsNullOrEmpty(sceneName)) {
                var status = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                if (status != SceneEventProgressStatus.Started) {
                    Debug.LogWarning($"{typeof(LoadSceneStep)}: Failed to load {sceneName} " + $"with a {nameof(SceneEventProgressStatus)}: {status}");
                }
            }
        }
        
        public void StartIntro() => LoadSceneSingle("BMBF_KnotbAR_INTRO");
        public void StartLevel1() => LoadSceneSingle("BMBF_KnotbAR_L1");
        public void StartLevel2()=> LoadSceneSingle("BMBF_KnotbAR_L2");
        public void StartLevel3() => LoadSceneSingle("BMBF_KnotbAR_L3");
        public void StartLevelChoice() => LoadSceneSingle("BMBF_KnotbAR_LEVELCHOICE");

    }
}
