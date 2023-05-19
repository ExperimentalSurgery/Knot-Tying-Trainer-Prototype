using System.Collections;
using System.Collections.Generic;
using NMY;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace DFKI.NMY
{
    public class UserInterfaceManager : SingletonStartupBehaviour<UserInterfaceManager>
    {

        [SerializeField] private ActivatableStartupBehaviour successPanel;
        [SerializeField] private TextMeshProUGUI stepTitle;
        [SerializeField] private TextMeshProUGUI stepDescription;

        public void ShowSuccessPanel()=>successPanel.Activate();
        public void HideSuccessPanel()=>successPanel.Deactivate();

        public void UpdateStepInfos(LocalizedString title, LocalizedString description)
        {
            stepTitle.text = title.GetLocalizedString();
            stepDescription.text = description.GetLocalizedString();

        }
        
    }
}
