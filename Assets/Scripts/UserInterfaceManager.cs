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

        [SerializeField] private ActivatableStartupBehaviour successPanel;
        [SerializeField] private GestureStepListView trainingStepListView;
        
        [Header("MRTK UI")]
        [SerializeField] private TextMeshPro stepTitleMrtk;
        [SerializeField] private TextMeshProUGUI stepDescriptionMrtk;
        [SerializeField] private ProgressIndicatorLoadingBar progressIndicatorMrtk;
        [SerializeField] private Interactable pauseButtonMrtk;
        [SerializeField] private Interactable forwardButtonMrtk;

        public void ShowSuccessPanel()=>successPanel.Activate();
        public void HideSuccessPanel()=>successPanel.Deactivate();

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
            trainingStepListView.gameObject.SetActive(!trainingStepListView.isActiveAndEnabled);
        }
        
        
        private async void OpenIndicator(IProgressIndicator indicator)
        {
            await indicator.AwaitTransitionAsync();
            await indicator.OpenAsync();
        }
        
    }
}
