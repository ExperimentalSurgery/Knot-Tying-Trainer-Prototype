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


        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.N)) TriggerNextStepIndex();
            if(Input.GetKeyDown(KeyCode.P)) TriggerPreviousStepIndex();
        }

        protected override void ActivateEnter()
        {
            GestureSequencePlayer.instance.LeftHandBvhFile = bvhFileLeft;
            GestureSequencePlayer.instance.RightHandBvhFile = bvhFileRight;
            GestureSequencePlayer.instance.InitSequence();
            base.ActivateEnter();
        }

        protected override void ActivateImmediatelyEnter()
        {
            GestureSequencePlayer.instance.LeftHandBvhFile = bvhFileLeft;
            GestureSequencePlayer.instance.RightHandBvhFile = bvhFileRight;
            GestureSequencePlayer.instance.InitSequence();
            base.ActivateImmediatelyEnter();
        }
    }
}
