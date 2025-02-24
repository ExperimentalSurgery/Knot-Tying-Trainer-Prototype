using System;
using System.Collections;
using System.Collections.Generic;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;
using UnityEngine.Assertions;

namespace DFKI.NMY
{
    public class ActivateByActiveChapter : MonoBehaviour
    {

        [SerializeField] private ChapterTrainingStep chapter;
        [SerializeField] private GameObject target;
        
        void Update() {
            
           
         
            if (chapter.stepState == BaseTrainingStep.StepState.StepStarted ||
                chapter.stepState == BaseTrainingStep.StepState.StepFinished) {
                
                this.target.SetActive(true);
            }
            else
            {
               
                this.target.SetActive(false);
            }
        
        }
    }
}
