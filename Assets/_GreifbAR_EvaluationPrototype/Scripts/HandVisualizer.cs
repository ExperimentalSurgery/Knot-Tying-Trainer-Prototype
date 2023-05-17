using System.Collections;
using System.Collections.Generic;
using NMY;
using UnityEngine;

namespace DFKI.NMY
{
    public class HandVisualizer : SingletonStartupBehaviour<HandVisualizer>
    {
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
        
        public Color errorColor = Color.red;
        public Color successColor = Color.green;
        public Color defaultColor = Color.blue;


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


        public void ResetColor()
        {
            userHandRenderer_left.material.SetFloat("_useOutline",0);
            userHandRenderer_right.material.SetFloat("_useOutline",0);
        }
        
        public void SetColor(Color c, bool left,bool right){
            if (left) {
                userHandRenderer_left.material.SetFloat("_useOutline",1);
                userHandRenderer_left.material.SetColor("_OutlineColor",c);
            }
            if (right) {
                userHandRenderer_right.material.SetFloat("_useOutline",1);
                userHandRenderer_right.material.SetColor("_OutlineColor",c);
            }
        }

        public void SetSuccessColor(bool left, bool right) => SetColor(successColor, left, right);
        public void SetErrorColor(bool left, bool right) => SetColor(errorColor, left, right);
        public void SetDefaultColor(bool left, bool right) => SetColor(defaultColor, left, right);

        
    }
}
