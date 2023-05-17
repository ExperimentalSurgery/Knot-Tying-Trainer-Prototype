using System.Collections;
using System.Collections.Generic;
using NMY;
using UnityEngine;

namespace DFKI.NMY
{
    public class UserInterfaceManager : SingletonStartupBehaviour<UserInterfaceManager>
    {

        [SerializeField] private ActivatableStartupBehaviour successPanel;

        public void ShowSuccessPanel()=>successPanel.Activate();
        public void HideSuccessPanel()=>successPanel.Deactivate();
        
    }
}
