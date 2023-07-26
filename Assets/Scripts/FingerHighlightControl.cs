using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DFKI.NMY
{
    [Serializable]
    public class FingerHighlightContainer
    {
        public FingerHighlightControl.Part Part;
        public FingerHighlightControl.Mode Mode;
        public bool LeftHand;
        public bool RightHand;
    }
    public class FingerHighlightControl : MonoBehaviour
    {
        public enum Part {Hand, Thumb, Thumb_Tip, Index, Index_Tip, Middle, Middle_Tip, Ring, Ring_Tip, Pinky, Pinky_Tip};
        public enum Mode {FlashOnce, Pulse, On, Off};

        public Animator handAnimatorRight;
        public Animator handAnimatorLeft;

        public SkinnedMeshRenderer rightHandRenderer;
        public SkinnedMeshRenderer leftHandRenderer;


        // Start is called before the first frame update
        void Start()
        {
            rightHandRenderer.material = new Material(rightHandRenderer.material);
            leftHandRenderer.material = new Material(leftHandRenderer.material);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        
        [ContextMenu("Right Middle FlashOnce")]
        public void TestTrigger()
        {
            SetHighlight(Part.Middle, Mode.FlashOnce, false);
        }
        
        [ContextMenu("Left Hand Pulse Toggle")]
        public void TestTrigger2()
        {
            SetHighlight(Part.Hand, Mode.Pulse, true);
        }
        
        [ContextMenu("Left Hand Index Tip Toggle")]
        public void TestTrigger3()
        {
            SetHighlight(Part.Index_Tip, Mode.Pulse, true);
        }

        [ContextMenu("Left Hand Ring Toggle")]
        public void TestTrigger4()
        {
            SetHighlight(Part.Ring, Mode.Pulse, true);
        }

        public void SetHighlight(Part partToHighlight, Mode mode, bool leftHand)
        {
            string triggerString = "";

            triggerString += partToHighlight.ToString() + mode.ToString();

            switch (mode)
            {
                case Mode.FlashOnce:
                    if(leftHand)
                        handAnimatorLeft.SetTrigger(triggerString);
                    else
                        handAnimatorRight.SetTrigger(triggerString);
                    break;
                case Mode.Pulse:
                    if(leftHand)
                        handAnimatorLeft.SetBool(triggerString, !handAnimatorLeft.GetBool(triggerString));
                    else
                        handAnimatorRight.SetBool(triggerString, !handAnimatorLeft.GetBool(triggerString));
                    break;
                case Mode.On:
                    if(leftHand)
                        handAnimatorLeft.SetBool(triggerString, true);
                    else
                        handAnimatorRight.SetBool(triggerString, true);
                    break;
                case Mode.Off:
                    if(leftHand)
                        handAnimatorLeft.SetBool(triggerString, false);
                    else
                        handAnimatorRight.SetBool(triggerString, false);
                    break;
                default:
                    break;
            }
        }
    }
}
