using NMY;
using NMY.VirtualRealityTraining;
using UnityEngine;
using System.Threading;
using NMY.VirtualRealityTraining.Steps;

namespace DFKI.NMY
{
    public class UserInterfaceManager : SingletonStartupBehaviour<UserInterfaceManager>
    {
  
        [Header("Control Panel Buttons")]
        [SerializeField] private GreifbarLeapInteractionButton previousStepBtn;
        [SerializeField] private GreifbarLeapInteractionButton speedToggleBtn;
        [SerializeField] private GreifbarLeapInteractionButton listViewToggleBtn;

        public BaseTrainingStep tutorialPreviousStep;
        
        private TrainingStepController mainStepController;
        
        protected override void StartupEnter()
        {
            base.StartupEnter();
            if(previousStepBtn) previousStepBtn.OnPress-=OnPreviousStepButtonClicked;
            if(previousStepBtn) previousStepBtn.OnPress+=OnPreviousStepButtonClicked;
            if(speedToggleBtn) speedToggleBtn.OnPress-=TogglePlaybackSpeed;
            if(speedToggleBtn) speedToggleBtn.OnPress+=TogglePlaybackSpeed;
            if(listViewToggleBtn) listViewToggleBtn.OnPress-=ToggleStepListView;
            if(listViewToggleBtn) listViewToggleBtn.OnPress+=ToggleStepListView;
            mainStepController = FindObjectOfType<TrainingStepController>();
        }

        public void ResetFingerHighlights()
        {
            HandVisualizer visualizer = GreifbARApp.instance.handVisualizer;
            foreach (var trigger in visualizer.handAnimatorUserLeft.parameters)
            {
                if (trigger.type == AnimatorControllerParameterType.Trigger)
                {
                    visualizer.handAnimatorUserLeft.ResetTrigger(trigger.name);
                    visualizer.handAnimatorUserRight.ResetTrigger(trigger.name);
                }
                else if (trigger.type == AnimatorControllerParameterType.Bool)
                {
                    visualizer.handAnimatorUserLeft.SetBool(trigger.name,false);
                    visualizer.handAnimatorUserRight.SetBool(trigger.name,false);
                }
            }
        }
        
        public void FingerHighlight(FingerHighlightContainer config) {
                HandVisualizer visualizer = GreifbARApp.instance.handVisualizer;
                Debug.Log("FingerHighlight");
                if (config.LeftHand) {
                    visualizer.SetHighlight(config.Part, config.Mode, true,config.UserHands,config.ExpertHands);
                }

                if (config.RightHand) {
                    visualizer.SetHighlight(config.Part, config.Mode, false,config.UserHands,config.ExpertHands);
                }
        }

        public void TogglePlaybackSpeed()
        {
            GreifbARApp.instance.gestureSequencePlayer.ToggleSpeed();
            GreifbARApp.instance.pointCloudPlayer.ToggleSpeed();
        }
        public void ToggleStepListView()
        {
            // As there will be several instances of it for different chapters we need to search and loop over those and configure
            var instances = FindObjectsOfType<GreifbarTrainingStepList>(includeInactive:true);
            foreach(var taskList  in instances) {
                if(taskList.chapterActive)
                    taskList.gameObject.SetActive(!taskList.gameObject.activeInHierarchy);
            }
        }
        
        public void OnPreviousStepButtonClicked() {
            if(mainStepController.currentStep == tutorialPreviousStep)
                return;
            CancellationToken stepKiller = new CancellationToken();
            mainStepController.currentStep.StopStepAction(true);
            mainStepController.currentStep.TryMoveToStep(mainStepController.previousStep, stepKiller);
        }
        
        
    }
}
