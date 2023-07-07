using Microsoft.MixedReality.Toolkit.UI;
using NMY;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

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
        
        
        protected override void StartupEnter()
        {
            base.StartupEnter();
            if(progressIndicatorMrtk) OpenIndicator(progressIndicatorMrtk);
            if(previouStepBtn) previouStepBtn.OnClick.AddListener(OnPreviousStepButtonClicked);
            if(speedToggleBtn) speedToggleBtn.OnClick.AddListener(TogglePlaybackSpeed);
            if(listViewToggleBtn) listViewToggleBtn.OnClick.AddListener(ToggleStepListView);
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
            
        }

        public void HideProgressIndicator() {
            if(progressIndicatorMrtk) progressIndicatorMrtk.gameObject.SetActive(false);
        }

        public void UpdateStepInfos(LocalizedString title, LocalizedString description)
        {
            if(stepDescriptionMrtk)stepTitleMrtk.text = title.GetLocalizedString();
            if(stepDescriptionMrtk)stepDescriptionMrtk.text = description.GetLocalizedString();
        }

        

        public void TogglePlaybackSpeed() => GestureSequencePlayer.instance.ToggleSpeed();

        public void ToggleStepListView() {
            
            //TODO: re impl
            
        }
        
        public void OnPreviousStepButtonClicked() {
            //TODO: Implement
        }
        
        private async void OpenIndicator(IProgressIndicator indicator)
        {
            await indicator.AwaitTransitionAsync();
            await indicator.OpenAsync();
        }
        
    }
}
