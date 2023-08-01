using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DFKI.NMY
{
    
    public enum KnotenbankDataType {Contact,Tension} 
    public class KnotenbankStep : GreifbarBaseStep {
        
        public enum ContactState{closeContact, openContact}

        [Header("KnotenbankStep")]
        [SerializeField] private KnotenbankDataType checkValue;
        [Range(0,350)]
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

        protected override async UniTask PreStepActionAsync(CancellationToken ct) {
            await base.PreStepActionAsync(ct);
            remainingDuration = minHoldDuration;
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

            Debug.Log("data: " + tmpInt + " " + tmpInt2);
        }
        protected override async UniTask PostStepActionAsync(CancellationToken ct) {
            await base.PostStepActionAsync(ct);
            remainingDuration = 0f;
            // Unsubscribe from SerialController Events
            if (knotBankSerialController) {
                knotBankSerialController.SerialMessageEventHandler -= OnMessageArrived;
            }
        }

        
        
        private void Update()
        {
            if (base.stepState.Equals(StepState.StepStarted)) {
                switch (checkValue) {
                    case KnotenbankDataType.Contact:
                        remainingDuration = contactVal == targetContactState ? (remainingDuration - Time.deltaTime) : minHoldDuration;
                        break;
                    case KnotenbankDataType.Tension:
                        remainingDuration = tensionVal >= targetTensionValue ? (remainingDuration - Time.deltaTime) : minHoldDuration;
                        break;
                }

                if (remainingDuration <= 0.0f ) {
                    FinishedCriteria = true;
                }
            }
        }
        
    }
}
