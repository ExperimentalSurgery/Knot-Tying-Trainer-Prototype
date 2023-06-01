using System;
using System.Collections;
using System.Collections.Generic;
using NMY.VTT.Core;
using UnityEngine;

namespace DFKI.NMY
{
    public class GestureScenario : VTTSimpleStepController
    {

        [Header("Knot Scenario Config")]
        [SerializeField] private string bvhFileLeft = ""; 
        [SerializeField] private string bvhFileRight = "";


        protected override void ActivateEnter()
        {
            GestureSequencePlayer.instance.LeftHandBvhFile = bvhFileLeft;
            GestureSequencePlayer.instance.RightHandBvhFile = bvhFileRight;
            GestureSequencePlayer.instance.InitSequence();
            base.ActivateEnter();
            if (UserInterfaceManager.instance.ForwardButtonMrtk)
            {
                UserInterfaceManager.instance.ForwardButtonMrtk.OnClick.AddListener(OnForwardTriggered);
            }
            if (UserInterfaceManager.instance.PauseButtonMrtk)
            {
                UserInterfaceManager.instance.PauseButtonMrtk.OnClick.AddListener(OnPauseTriggered);
            }
            
        }
        protected override void ActivateImmediatelyEnter() => ActivateEnter();

        protected override void DeactivateEnter()
        {
            base.DeactivateEnter();
            if (UserInterfaceManager.instance.ForwardButtonMrtk)
            {
                UserInterfaceManager.instance.ForwardButtonMrtk.OnClick.RemoveListener(OnForwardTriggered);
            }
            if (UserInterfaceManager.instance.PauseButtonMrtk)
            {
                UserInterfaceManager.instance.PauseButtonMrtk.OnClick.RemoveListener(OnPauseTriggered);
            }
        }

        protected override void DeactivateImmediatelyEnter() => DeactivateEnter();

        private void OnForwardTriggered()
        {
            GestureBaseStep step = (entrys[currentStepIndex] as GestureBaseStep);
            step.RaiseStepCompletedEvent();
        }

        private void OnPauseTriggered()
        {
            if (GestureSequencePlayer.instance.isPlayingLeft && GestureSequencePlayer.instance.isPlayingRight)
            {
                GestureSequencePlayer.instance.Pause();
            }
            else
            {
                GestureSequencePlayer.instance.Resume();
            }

        }

       
    }
}
