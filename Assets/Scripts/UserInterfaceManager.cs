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
        [SerializeField] private TextMeshProUGUI stepTitle;
        [SerializeField] private TextMeshProUGUI stepDescription;

        [SerializeField] private Scrollbar playbackSpeedScrollbar;
        [SerializeField] private Image progressFillLeft;
        [SerializeField] private Image progressFillRight;
        
        [Header("MRTK UI")]
        [SerializeField] private TextMeshPro stepTitleMrtk;
        [SerializeField] private TextMeshProUGUI stepDescriptionMrtk;
        [SerializeField] private ProgressIndicatorLoadingBar progressIndicatorMrtk;
        [SerializeField] private PinchSlider playbackSpeedSliderMrtk;
        [SerializeField] private Interactable pauseButtonMrtk;
        [SerializeField] private Interactable forwardButtonMrtk;

        public void ShowSuccessPanel()=>successPanel.Activate();
        public void HideSuccessPanel()=>successPanel.Deactivate();

        public void UpdateStepInfos(LocalizedString title, LocalizedString description)
        {
            if(stepTitle)stepTitle.text = title.GetLocalizedString();
            if(stepDescription)stepDescription.text = description.GetLocalizedString();
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

        public PinchSlider PlaybackSpeedSliderMrtk
        {
            get => playbackSpeedSliderMrtk;
            set => playbackSpeedSliderMrtk = value;
        }

        private void Update()
        {
            GestureSequencePlayer player = GestureSequencePlayer.instance;
            if (player.isPlayingLeft){
                if(progressIndicatorMrtk) progressIndicatorMrtk.Progress =player.PlayAllSequences ? player.normalizedProgressTotalLeft : player.normalizedProgressLeft;
                
                if(progressFillLeft)progressFillLeft.fillAmount = player.PlayAllSequences ? player.normalizedProgressTotalLeft : player.normalizedProgressLeft;
            }
            if (player.isPlayingRight) {
                
                if(progressIndicatorMrtk) progressIndicatorMrtk.Progress =player.PlayAllSequences ? player.normalizedProgressTotalRight : player.normalizedProgressRight;
                if(progressFillRight)progressFillRight.fillAmount = player.PlayAllSequences ? player.normalizedProgressTotalRight : player.normalizedProgressRight;
            }
            
            

        }
        
        private async void OpenIndicator(IProgressIndicator indicator)
        {
            await indicator.AwaitTransitionAsync();
            await indicator.OpenAsync();
        }
        
    }
}
