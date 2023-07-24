using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using NMY;
using UnityEngine;

namespace DFKI.NMY
{
    public class PreviewHands : SingletonStartupBehaviour<PreviewHands>
    {
        public Animator handsAnimator;
        public string activeState;
        public bool isActivated {get;  private set;}

        private int enabledStateID;
        private bool isInitialized;

        private void Start() 
        {
            if (handsAnimator == null)
            {
                handsAnimator = GetComponentInChildren<Animator>();
                if(handsAnimator == null)
                {
                    Debug.LogError("missing animator");
                    return;
                }
            }
            if (String.IsNullOrEmpty(activeState))
            {
                Debug.LogError("missing state name");
                return;
            }
                enabledStateID = Animator.StringToHash(activeState);                

            isInitialized = true;
        }

        public void Activate()
        {
            if(isActivated)
                return;
            
            // Do stuff
            handsAnimator.SetBool(enabledStateID, true);

            isActivated = true;
        }

        public void Deactivate()
        {
            if(!isActivated)
                return;
            
            // Do stuff
            handsAnimator.SetBool(enabledStateID, false);

            isActivated = false;
        }


    }
}
