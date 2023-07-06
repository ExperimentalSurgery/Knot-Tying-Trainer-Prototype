using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using NMY;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace DFKI.NMY
{
    public class UserInterfaceManager : SingletonStartupBehaviour<UserInterfaceManager>
    {
        
        [SerializeField] private GestureTrainingController mainTrainingController;
        [SerializeField] private ActivatableStartupBehaviour successPanel;
        [SerializeField] private GestureStepListView trainingStepListView;
        
        [Header("MRTK UI")]
        [SerializeField] private TextMeshPro stepTitleMrtk;
        [SerializeField] private TextMeshProUGUI stepDescriptionMrtk;
        [SerializeField] private ProgressIndicatorLoadingBar progressIndicatorMrtk;
        [SerializeField] private Interactable pauseButtonMrtk;
        [SerializeField] private Interactable forwardButtonMrtk;
        
        [Header("Control Panel Buttons")]
        [SerializeField] private Interactable previouStepBtn;
        [SerializeField] private Interactable speedToggleBtn;
        [SerializeField] private Interactable listViewToggleBtn;
        
        
        
        public void ShowSuccessPanel() {
            if (successPanel) successPanel.Activate();
        }

        public void HideSuccessPanel() {
            if(successPanel) successPanel.Deactivate();
        }

        public void ShowProgressIndicator()
        {
            if(progressIndicatorMrtk) progressIndicatorMrtk.gameObject.SetActive(true);
            
        }

        public void HideProgressIndicator() {
            if(progressIndicatorMrtk) progressIndicatorMrtk.gameObject.SetActive(false);
        }

        public void UpdateStepInfos(LocalizedString title, LocalizedString description)
        {
            if(stepDescriptionMrtk)stepTitleMrtk.text = title.GetLocalizedString();
            if(stepDescriptionMrtk)stepDescriptionMrtk.text = description.GetLocalizedString();
        }

        protected override void StartupEnter()
        {
            base.StartupEnter();
            if(progressIndicatorMrtk) OpenIndicator(progressIndicatorMrtk);
        }


        public Interactable PauseButtonMrtk
        {
            get => pauseButtonMrtk;
            set => pauseButtonMrtk = value;
        }

        public Interactable ForwardButtonMrtk
        {
            get => forwardButtonMrtk;
            set => forwardButtonMrtk = value;
        }
        
        private void Update()
        {
            GestureSequencePlayer player = GestureSequencePlayer.instance;
            if (player.isPlayingLeft){
                if(progressIndicatorMrtk) progressIndicatorMrtk.Progress =player.PlayAllSequences ? player.normalizedProgressTotalLeft : player.normalizedProgressLeft;
            }
            if (player.isPlayingRight) {
                if(progressIndicatorMrtk) progressIndicatorMrtk.Progress = player.PlayAllSequences ? player.normalizedProgressTotalRight : player.normalizedProgressRight;
            }
        }

        public void TogglePlaybackSpeed() => GestureSequencePlayer.instance.ToggleSpeed();

        public void ToggleStepListView()
        {
            if(trainingStepListView) trainingStepListView.Activate(!trainingStepListView.isActivated);
        }
        
        public void OnPreviousStepButtonClicked() => mainTrainingController.GoToPreviousStep();
        
        private async void OpenIndicator(IProgressIndicator indicator)
        {
            await indicator.AwaitTransitionAsync();
            await indicator.OpenAsync();
        }
        
    }
}
