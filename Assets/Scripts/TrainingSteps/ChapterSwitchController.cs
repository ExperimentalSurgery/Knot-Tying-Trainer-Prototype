using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NMY.VirtualRealityTraining;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;

namespace DFKI.NMY
{
    public class ChapterSwitchController : MonoBehaviour
    {
        [SerializeField] private TrainingStepController mainStepController;
        
        [SerializeField] private ChapterTrainingStep level1;
        [SerializeField] private ChapterTrainingStep level2;
        [SerializeField] private ChapterTrainingStep level3;
        [SerializeField] private BaseTrainingStep parentChapter;


        public async void PreviousStep()
        {
            if (mainStepController.previousStep == null) return;
            CancellationToken stepKiller = new CancellationToken();
            mainStepController.currentStep.StopStepAction(true);
            bool success = await parentChapter.TryMoveToStep(mainStepController.previousStep, stepKiller);
        }

        /*public async void NextStep()
        {
            CancellationToken stepKiller = new CancellationToken();
            mainStepController.currentStep.StopStepAction(false);
        }*/
        
        public async void RepeatStep() {
            CancellationToken stepKiller = new CancellationToken();
            mainStepController.currentStep.StopStepAction(false);
            bool success = await parentChapter.TryMoveToStep(mainStepController.currentStep, stepKiller);
        }
        
        public async void SwitchTo(int c) {
            Debug.Log("attempt switch to Chapter "+c);
            if (c == 1)
            {
                CancellationToken stepKiller = new CancellationToken();
                bool success = await parentChapter.TryMoveToStep(level1, stepKiller);
                Debug.Log("try change "+success);
            }
            else if (c == 2)
            {
                CancellationToken stepKiller = new CancellationToken();
                bool success = await parentChapter.TryMoveToStep(level2, stepKiller);
                Debug.Log("try change "+success);
            }
            else if (c == 3)
            {
                CancellationToken stepKiller = new CancellationToken();
                bool success = await parentChapter.TryMoveToStep(level3, stepKiller);
                Debug.Log("try change "+success);
            }
            
            //mainStepController.TryMoveToStep(chapter2, chapter1);
            //chapter.TryMoveToStep(targetStep, stepKiller).Forget();
            //CancellationToken stepKiller = new CancellationToken();
            //Debug.Log("current step "+mainStepController.currentStep.name);
            //
            //mainStepController.currentStep.TryMoveToStep(target, stepKiller);


        }
    }
}
