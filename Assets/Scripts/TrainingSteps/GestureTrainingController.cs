using System;
using System.Collections;
using System.Collections.Generic;
using NMY.VTT.Core;
using UnityEngine;

namespace DFKI.NMY
{
    public class GestureTrainingController : VTTIndexBasedListController
    {
        
        [Header("Simple Step Controller")]
        public List<VTTSimpleStepController> trainings;

        protected override void CopyToBaseArray()
        {
            for (int i = 0; i < trainings.Count; i++) { base.entrys.Add(i, trainings[i]); }
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                GoToPreviousStep();
            }
        }

        public void ResetCurrentScenario() {
            GestureScenario currentScenario = trainings[currentStepIndex] as GestureScenario;
            if (currentScenario) {
                currentScenario.ResetListController();
                currentScenario.Activate();
            }
        }

        public void GoToPreviousStep() {
            GestureScenario currentScenario = trainings[currentStepIndex] as GestureScenario;
            int targetIndex = currentScenario.GetPreviousStepIndex();
            currentScenario.ResetListController();
            currentScenario.Activate();
            
            int index = 0;
            for (int i = 0; i < targetIndex;i++) {
                currentScenario.trainingSteps[i].RaiseStepCompletedEvent();
            }
        }
        
        
        
        protected override void OnControllerReset() {
        }

        protected override void HandleEntryActivate(object sender, ListControllerEventArgs args)
        {
           
        }

        protected override void HandleEntryCompleted(object sender, ListControllerEventArgs args)
        {
           
        }

        protected override void HandleEntryDeactivate(object sender, ListControllerEventArgs args)
        {
           
        }

        protected override void HandleListCompleted(object sender, ListControllerEventArgs args)
        {
            
        }
        
    }
}
