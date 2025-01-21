
using System;
using NMY;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace DFKI.NMY
{
    public class LevelChoicePanel : SimpleOnOffActivatable
    {

        [SerializeField] private GreifbARWorldSpaceButton introBtn;
        [SerializeField] private GreifbARWorldSpaceButton level1Btn;
        [SerializeField] private GreifbARWorldSpaceButton level2Btn;
        [SerializeField] private GreifbARWorldSpaceButton level3Btn;

        public UnityEvent<int> levelSwitchTriggered = new UnityEvent<int>();


        private void OnEnable()
        {
            introBtn.Interactable.selectEntered.AddListener(OnSelectedLevel);  
            level1Btn.Interactable.selectEntered.AddListener(OnSelectedLevel);  
            level2Btn.Interactable.selectEntered.AddListener(OnSelectedLevel);  
            level3Btn.Interactable.selectEntered.AddListener(OnSelectedLevel);  
        }

        private void OnDisable()
        {
            introBtn.Interactable.selectEntered.RemoveListener(OnSelectedLevel);  
            level1Btn.Interactable.selectEntered.RemoveListener(OnSelectedLevel);  
            level2Btn.Interactable.selectEntered.RemoveListener(OnSelectedLevel);  
            level3Btn.Interactable.selectEntered.RemoveListener(OnSelectedLevel);  
         
        }

        private void OnSelectedLevel(SelectEnterEventArgs args)
        {
            if (args.interactableObject == introBtn.Interactable) {
                levelSwitchTriggered.Invoke(0);
            }
            else if (args.interactableObject == level1Btn.Interactable) {
                levelSwitchTriggered.Invoke(1);
            }
            else if (args.interactableObject == level2Btn.Interactable) {
                levelSwitchTriggered.Invoke(2);
            }
            else if (args.interactableObject == level3Btn.Interactable) {
                levelSwitchTriggered.Invoke(3);
            }
        }
        
    }
}
