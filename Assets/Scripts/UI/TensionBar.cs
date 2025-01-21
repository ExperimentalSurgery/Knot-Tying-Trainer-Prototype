using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DFKI.NMY
{
    public class TensionBar : MonoBehaviour
    {
        public Image fill;

        public Color tooLess;
        public Color optimal;
        public Color tooMuch;

        public float tooLessThreshold = 0.375f;
        public float optimalMax = 0.625f; 

    

        void Update(){
            

                if(fill.fillAmount<tooLessThreshold){
                    fill.color = tooLess;
                }
                else if(fill.fillAmount<optimalMax){
                    fill.color = optimal;
                }
                else if(fill.fillAmount> optimalMax){
                    fill.color = tooMuch;
                }
                

        }
    }
}
