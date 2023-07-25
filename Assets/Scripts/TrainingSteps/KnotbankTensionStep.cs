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
        
        // private vars
        private int tmpInt;
        private int contactVal;
        private float remainingDuration;
        
        [SerializeField] private float minHoldDuration = 2f;

        protected override async UniTask PreStepActionAsync(CancellationToken ct) {
            await base.PreStepActionAsync(ct);
            remainingDuration = minHoldDuration;
        }
        
        protected override async UniTask PostStepActionAsync(CancellationToken ct) {
            await base.PostStepActionAsync(ct);
            remainingDuration = 0f;
        }

        // Invoked when a line of data is received from the serial device.
        public void OnMessageArrived(string msg) {
            string[] data = msg.Split(';');
            //Debug.Log($"Contact: {data[0]} - Tension Grams: {data[1]}");

            if (int.TryParse(data[0], out tmpInt)) {
                contactVal = tmpInt;
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
