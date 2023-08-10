using System;
using System.Collections;
using System.Collections.Generic;
using NMY;
using UnityEngine;

namespace DFKI.NMY
{
    [Serializable]
    public class FingerHighlightContainer
    {
        public HandVisualizer.Part Part;
        public HandVisualizer.Mode Mode;
        public bool LeftHand=true;
        public bool RightHand=true;
        public bool ExpertHands=true;
        public bool UserHands=true;
    }
    
    public class HandVisualizer : SingletonStartupBehaviour<HandVisualizer>
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

        
        [Header("HandVisualizer")]
        [Tooltip("GameObject of left user hand")]
        public GameObject leftUserHand;
        [Tooltip("GameObject of left user hand")]
        public GameObject rightUserHand;
        
        [Tooltip("GameObject of left expert hand")]
        public GameObject leftExpertHand;
        [Tooltip("GameObject of left expert hand")]
        public GameObject rightExpertHand;

        public Renderer userHandRenderer_left;
        public Renderer userHandRenderer_right;
        
        [Header("Color Config")]
        public Color errorColor = Color.red;
        public Color successColor = Color.green;
        public Color defaultColor = Color.blue;

        protected override void StartupEnter()
        {
            base.StartupEnter();
            rightHandRenderer.material = new Material(rightHandRenderer.material);
            leftHandRenderer.material = new Material(leftHandRenderer.material);
        }

        #region Outlines

        public void SetUserHandVisibleLeft(bool visible)
        {
            leftUserHand.gameObject.SetActive(visible);
        }
        
        public void SetUserHandVisibleRight(bool visible)
        {
            rightUserHand.gameObject.SetActive(visible);
        }

        public void SetExpertHandVisibleLeft(bool visible)
        {
            rightExpertHand.gameObject.SetActive(visible);
        }
        
        public void SetExpertHandVisibleRight(bool visible)
        {
            leftExpertHand.gameObject.SetActive(visible);
        }


        public void ResetOutline()
        {
            userHandRenderer_left.material.SetFloat("_useOutline",0);
            userHandRenderer_right.material.SetFloat("_useOutline",0);
        }
        
        public void SetOutlineColor(Color c, bool left,bool right){
            if (left) {
                userHandRenderer_left.material.SetFloat("_useOutline",1);
                userHandRenderer_left.material.SetColor("_OutlineColor",c);
            }
            if (right) {
                userHandRenderer_right.material.SetFloat("_useOutline",1);
                userHandRenderer_right.material.SetColor("_OutlineColor",c);
            }
        }

        public void SetTimedSuccessOutline(float duration,bool left,bool right)
        {
            ResetOutline();
            StopAllCoroutines();
            StartCoroutine(TimedOutline(duration,left,right));

        }

        protected IEnumerator TimedOutline(float seconds,bool left,bool right)
        {
            SetSuccessOutline(left,right);
            yield return new WaitForSeconds(seconds);
            ResetOutline();
        }

        public void SetSuccessOutline(bool left, bool right) => SetOutlineColor(successColor, left, right);
        public void SetErrorOutline(bool left, bool right) => SetOutlineColor(errorColor, left, right);
        public void SetDefaultOutline(bool left, bool right) => SetOutlineColor(defaultColor, left, right);

        #endregion


        #region Highlighting

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
        

        #endregion
        
        
    }
}
