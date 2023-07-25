using Microsoft.MixedReality.Toolkit.UI;
using NMY;
using NMY.VirtualRealityTraining;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using System.Threading;
using NMY.VirtualRealityTraining.Steps;

namespace DFKI.NMY
{
    public class UserInterfaceManager : SingletonStartupBehaviour<UserInterfaceManager>
    {
        
        [Header("References")]
        [SerializeField] private ActivatableStartupBehaviour successPanel;
        [SerializeField] private TextMeshPro stepTitleMrtk;
        [SerializeField] private TextMeshProUGUI stepDescriptionMrtk;
        [SerializeField] private ProgressIndicatorLoadingBar progressIndicatorMrtk;
        
        [Header("Control Panel Buttons")]
        [SerializeField] private GreifbarMRTKInteractable previouStepBtn;
        [SerializeField] private GreifbarMRTKInteractable speedToggleBtn;
        [SerializeField] private GreifbarMRTKInteractable listViewToggleBtn;

        public BaseTrainingStep tutorialPreviousStep;

        private TrainingStepController mainStepController;

        private bool taskListVisible = false;
        
        protected override void StartupEnter()
        {
            base.StartupEnter();
            if(progressIndicatorMrtk) OpenIndicator(progressIndicatorMrtk);
            if(previouStepBtn) previouStepBtn.OnClick.AddListener(OnPreviousStepButtonClicked);
            if(speedToggleBtn) speedToggleBtn.OnClick.AddListener(TogglePlaybackSpeed);
            if(listViewToggleBtn) listViewToggleBtn.OnClick.AddListener(ToggleStepListView);
            mainStepController = FindObjectOfType<TrainingStepController>();
        }

        
        public void ShowSuccessPanel() {
            if (successPanel) successPanel.Activate();
        }

        public void HideSuccessPanel() {
            if(successPanel) successPanel.Deactivate();
        }

        public void ShowProgressIndicator()
        {
            if(progressIndicatorMrtk) progressIndicatorMrtk.gameObject.SetActive(true);
            Debug.Log("ShowProgressIndicator");
        }

        public void HideProgressIndicator() {
            if(progressIndicatorMrtk) progressIndicatorMrtk.gameObject.SetActive(false);
             Debug.Log("HideProgressIndicator");
        }

        public void UpdateStepInfos(LocalizedString title, LocalizedString description)
        {
            if(stepTitleMrtk && !title.IsEmpty) stepTitleMrtk.text = title.GetLocalizedString();
            if(stepDescriptionMrtk && !description.IsEmpty) stepDescriptionMrtk.text = description.GetLocalizedString();
            
        }

        

        public void TogglePlaybackSpeed() => GestureSequencePlayer.instance.ToggleSpeed();

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
        
        private async void OpenIndicator(IProgressIndicator indicator)
        {
            await indicator.AwaitTransitionAsync();
            await indicator.OpenAsync();
        }
        
    }
}
