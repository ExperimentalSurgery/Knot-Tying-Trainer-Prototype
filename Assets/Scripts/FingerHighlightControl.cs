using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DFKI.NMY
{
    [Serializable]
    public class FingerHighlightContainer
    {
        
        
        public FingerHighlightControl.Part Part;
        public FingerHighlightControl.Mode Mode;
        public bool LeftHand=true;
        public bool RightHand=true;
        public bool ExpertHands=true;
        public bool UserHands=true;
    }
    public class FingerHighlightControl : MonoBehaviour
    {
        
        public bool allowHighlights = true;
        
        public enum Part {Hand, Thumb, Thumb_Tip, Index, Index_Tip, Middle, Middle_Tip, Ring, Ring_Tip, Pinky, Pinky_Tip};
        public enum Mode {FlashOnce, Pulse, On, Off};

        public Animator handAnimatorUserRight;
        public Animator handAnimatorUserLeft;
        public Animator handAnimatorExpertRight;
        public Animator handAnimatorExpertLeft;

        public SkinnedMeshRenderer rightHandRenderer;
        public SkinnedMeshRenderer leftHandRenderer;


        // Start is called before the first frame update
        void Start()
        {
            rightHandRenderer.material = new Material(rightHandRenderer.material);
            leftHandRenderer.material = new Material(leftHandRenderer.material);
        }

        
        [ContextMenu("Right Middle FlashOnce")]
        public void TestTrigger()
        {
            SetHighlight(Part.Middle, Mode.FlashOnce, false,handAnimatorUserLeft,handAnimatorUserRight);
        }
        
        [ContextMenu("Left Hand Pulse Toggle")]
        public void TestTrigger2()
        {
            SetHighlight(Part.Hand, Mode.Pulse, true,handAnimatorUserLeft,handAnimatorUserRight);
        }
        
        [ContextMenu("Left Hand Index Tip Toggle")]
        public void TestTrigger3()
        {
            SetHighlight(Part.Index_Tip, Mode.Pulse, true,handAnimatorUserLeft,handAnimatorUserRight);
        }

        [ContextMenu("Left Hand Ring Toggle")]
        public void TestTrigger4()
        {
            SetHighlight(Part.Ring, Mode.Pulse, true,handAnimatorUserLeft,handAnimatorUserRight);
        }

        public void SetHighlight(Part partToHighlight, Mode mode, bool leftHand, bool userHands, bool expertHands)
        {
            if (userHands)
            {
                SetHighlight(partToHighlight,mode,leftHand,handAnimatorUserLeft,handAnimatorUserRight);
            }

            if (expertHands)
            {
                SetHighlight(partToHighlight,mode,leftHand,handAnimatorExpertLeft,handAnimatorExpertRight);
                
            }
        }
        
        
        protected void SetHighlight(Part partToHighlight, Mode mode, bool leftHand,Animator animatorLeft,Animator animatorRight)
        {
            if (!allowHighlights) return;
            
            string triggerString = "";

            triggerString += partToHighlight.ToString() + mode.ToString();

            switch (mode)
            {
                case Mode.FlashOnce:
                    if(leftHand)
                        animatorLeft.SetTrigger(triggerString);
                    else
                        animatorRight.SetTrigger(triggerString);
                    break;
                case Mode.Pulse:
                    if(leftHand)
                        animatorLeft.SetBool(triggerString, !animatorLeft.GetBool(triggerString));
                    else
                        animatorRight.SetBool(triggerString, !animatorRight.GetBool(triggerString));
                    break;
                case Mode.On:
                    if(leftHand)
                        animatorLeft.SetBool(triggerString, true);
                    else
                        animatorRight.SetBool(triggerString, true);
                    break;
                case Mode.Off:
                    if(leftHand)
                        animatorLeft.SetBool(triggerString, false);
                    else
                        animatorRight.SetBool(triggerString, false);
                    break;
                default:
                    break;
            }
        }
    }
}
