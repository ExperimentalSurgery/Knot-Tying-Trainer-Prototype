using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DFKI.NMY
{
    

    public class KnotenbankStep : GreifbarBaseStep,IKnotbAR {
        
        [Header("KnotenbankStep")]
        [SerializeField] private KnotenbankDataType checkValue;

        [Range(0, 350)] [SerializeField] private float allowedDeviation = 50;
        [SerializeField] private int targetTensionValue = 350;
        [SerializeField] private ContactState targetContactState;
        [SerializeField] private float minHoldDuration = 2f;

        // runtime vars
        private int tmpInt;
        private int tmpInt2;
        private ContactState contactVal;
        private int tensionVal;
        private float remainingDuration;
        private SerialController knotBankSerialController;

        protected override void Reset()
        {
            base.Reset();
            this.name = "[KnotenbankStep] ";
        }
        
        
        protected override async UniTask ClientStepActionAsync(CancellationToken ct)
        {
            
            var includePhases = new List<TrainingPhase>();
            includePhases.Add(TrainingPhase.L2_KNOTENSPANNUNG);
            includePhases.Add(TrainingPhase.L3_ZEITMESSUNG);
            
            Debug.Log("IncludeCheck="+includePhases.Contains(Phase) 
            + " P="+Phase);

            // Check if we can skip the step due to not checked phase
            FinishedCriteria = !includePhases.Contains(Phase);
            await base.ClientStepActionAsync(ct);
        
         
        }

        protected override async UniTask PreStepActionAsync(CancellationToken ct) {
            await base.PreStepActionAsync(ct);
            remainingDuration = minHoldDuration;
            
            UserInterfaceManager.instance.tensionMeterFill.Activate();
            UserInterfaceManager.instance.tensionMeterFill.fillIcon.fillAmount = 0.0f;
            
            // Search for SerialController and register Events
            knotBankSerialController = SerialController.instance;
            if (knotBankSerialController) {
                knotBankSerialController.SerialMessageEventHandler -= OnMessageArrived;
                knotBankSerialController.SerialMessageEventHandler += OnMessageArrived;
            }
        }
        // Invoked when a line of data is received from the serial device.
        private void OnMessageArrived(object sender, MessageEventArgs e) {
            string[] data = e.message.Split(';');
            if (int.TryParse(data[0], out tmpInt)) {
                if(tmpInt == 0 || tmpInt == 1)
                    contactVal = (ContactState)tmpInt;
            }
            if (int.TryParse(data[1], out tmpInt2)) {
                tensionVal = tmpInt2 * -1;
            }
            
            Debug.Log("Set fill "+ (1.0f- (remainingDuration / minHoldDuration)));
            UserInterfaceManager.instance.tensionMeterFill.fillIcon.fillAmount = 1.0f- (remainingDuration / minHoldDuration);

            //Debug.Log("data: " + tmpInt + " " + tmpInt2);
        }
        protected override async UniTask PostStepActionAsync(CancellationToken ct) {
            await base.PostStepActionAsync(ct);
            remainingDuration = 0f;
            // Unsubscribe from SerialController Events
            if (knotBankSerialController) {
                knotBankSerialController.SerialMessageEventHandler -= OnMessageArrived;
            }
            
            UserInterfaceManager.instance.tensionMeterFill.Deactivate();
        }

        
        
        private void Update()
        {
            if (base.stepState.Equals(StepState.StepStarted)) {
                switch (checkValue) {
                    case KnotenbankDataType.Contact:
                        remainingDuration = contactVal == targetContactState ? (remainingDuration - Time.deltaTime) : minHoldDuration;
                        break;
                    case KnotenbankDataType.Tension:
                        remainingDuration = (tensionVal >= targetTensionValue-allowedDeviation) && (tensionVal <= targetTensionValue+allowedDeviation) ? (remainingDuration - Time.deltaTime) : minHoldDuration;
                        break;
                }
                
              
                if (remainingDuration <= 0.0f ) {
                    FinishedCriteria = true;
                }
            }
        }
        
    }
}
