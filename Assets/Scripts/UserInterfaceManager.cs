using System.Collections.Generic;
using NMY;
using NMY.VirtualRealityTraining;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace DFKI.NMY
{
    public class UserInterfaceManager : SingletonStartupBehaviour<UserInterfaceManager>
    {

        [Header("Analysis Panel")] [SerializeField]
        public TensionTimeFill tensionMeterFill;

        [SerializeField] public GameObject completionPanel;
        [SerializeField] public GameObject anaylsisPanel;
        [SerializeField] public ActivatableStartupBehaviour tensionMeterRoot;

        [Header("World Space Buttons")] [SerializeField]
        public GreifbARWorldSpaceButton backToLevelChoiceBtn;

        [SerializeField] public GreifbARWorldSpaceButton speedToggleBtn;
        [SerializeField] public GreifbARWorldSpaceButton listViewToggleBtn;
        [SerializeField] public GreifbARWorldSpaceButton skipStepBtn;
        [SerializeField] public TrainingStepController mainStepController;

        [Header("Feedbackpanel")] [SerializeField]
        public GreifbARWorldSpaceButton openAnalysisView;

        [Header("Invalid steps")] [SerializeField]
        private List<BaseTrainingStep> skipActionSteps;

        [Header("TaskList")] [SerializeField] private GameObject taskList;


        protected override void StartupEnter()
        {
            base.StartupEnter();
            if (speedToggleBtn) speedToggleBtn.Interactable.selectEntered.AddListener(TogglePlaybackSpeed);
            if (listViewToggleBtn) listViewToggleBtn.Interactable.selectEntered.AddListener(ToggleStepListView);
            if (skipStepBtn) skipStepBtn.Interactable.selectEntered.AddListener(ContinueStepButtonClicked);
            if (backToLevelChoiceBtn)
                backToLevelChoiceBtn.Interactable.selectEntered.AddListener(BackToLevelChoiceClicked);
            if (openAnalysisView) openAnalysisView.Interactable.selectEntered.AddListener(OnAnalysisButtonClicked);
        }




        private void BackToLevelChoiceClicked(SelectEnterEventArgs args)
        {
            GreifbARApp.instance.StartLevelChoice();
        }

        private void OnAnalysisButtonClicked(SelectEnterEventArgs arg0)
        {
            if (anaylsisPanel.gameObject.activeSelf)
            {
                anaylsisPanel.gameObject.SetActive(false);
                completionPanel.gameObject.SetActive(true);
            }
            else
            {
                anaylsisPanel.gameObject.SetActive(true);
                completionPanel.gameObject.SetActive(false);
            }
        }

        public void ActivateTensionMeterByPhase(TrainingPhase phase)
        {

            switch (phase)
            {
                case TrainingPhase.L0_INTRO:
                    ShowTensionMeter(false);
                    break;
                case TrainingPhase.L1_KNOTENTECHNIK:
                    ShowTensionMeter(true);
                    break;
                case TrainingPhase.L2_KNOTENSPANNUNG:
                    ShowTensionMeter(true);
                    break;
                case TrainingPhase.L3_ZEITMESSUNG:
                    ShowTensionMeter(true);
                    break;
            }
        }

        public void ShowTensionMeter(bool visible)
        {
            if (tensionMeterRoot)
            {
                tensionMeterRoot.Activate(visible);
            }
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
                    visualizer.handAnimatorUserLeft.SetBool(trigger.name, false);
                    visualizer.handAnimatorUserRight.SetBool(trigger.name, false);
                }
            }
        }

        public void FingerHighlight(FingerHighlightContainer config)
        {
            HandVisualizer visualizer = GreifbARApp.instance.handVisualizer;
            if (config.LeftHand)
            {
                visualizer.SetHighlight(config.Part, config.Mode, true, config.UserHands, config.ExpertHands);
            }

            if (config.RightHand)
            {
                visualizer.SetHighlight(config.Part, config.Mode, false, config.UserHands, config.ExpertHands);
            }
        }

        private void ToggleStepListView(SelectEnterEventArgs args)
        {

            taskList.gameObject.SetActive(!taskList.activeSelf);

        }

        private void TogglePlaybackSpeed(SelectEnterEventArgs args)
        {
            GreifbARApp.instance.gestureSequencePlayer.ToggleSpeed();
            GreifbARApp.instance.pointCloudPlayer.ToggleSpeed();
        }

        public void ContinueStepButtonClicked(SelectEnterEventArgs args)
        {

            GreifbARApp.instance.SkipGestureTriggered.Invoke();
            //if(skipActionSteps.Contains(mainStepController.currentStep)){return;}
            //switchController.NextStep();
        }


        public void SetContinueButtonVisible(bool visible)
        {
            if (skipStepBtn != null)
            {
                skipStepBtn.gameObject.SetActive(visible);
            }
        }

        public void ManuallyTriggerNextStep() => ContinueStepButtonClicked(null);
        public void ManuallyTriggerBackToLevelChoice() => BackToLevelChoiceClicked(null);
        public void ManuallyTriggerToggleStepList() => ToggleStepListView(null);
        public void ManuallyTriggerTogglePlaybackSpeed() => TogglePlaybackSpeed(null);

    }
}