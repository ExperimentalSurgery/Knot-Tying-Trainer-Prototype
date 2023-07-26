using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DFKI.NMY
{
    public class KnotbankTensionStep : GreifbarBaseStep
    {
        
        
        [Header("Knotbank TensionStep")]
        [Range(0,1)]
        [SerializeField] private int targetValue = 1;
        [SerializeField] private float minHoldDuration = 2f;

        // runtime vars
        private int tmpInt;
        private int contactVal;
        private float remainingDuration;
        private SerialController knotBankSerialController;
        
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
                contactVal = tmpInt;
            }
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
            if (base.stepState.Equals(StepState.StepStarted))
            {
                remainingDuration = contactVal == targetValue ? (remainingDuration - Time.deltaTime) : minHoldDuration;

                if (remainingDuration <= 0.0f ) {
                    FinishedCriteria = true;
                }
            }
        }
        
    }
}
