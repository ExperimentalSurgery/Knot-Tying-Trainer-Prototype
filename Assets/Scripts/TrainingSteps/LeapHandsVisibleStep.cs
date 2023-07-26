using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Leap.Unity;
using UnityEngine;
using UnityEngine.Serialization;

namespace DFKI.NMY
{
    public class LeapHandsVisibleStep : GreifbarBaseStep
    {
        [Header("Leap Hands Visible Step")]
        [SerializeField] private float minTrackedDuration = 2f;
        [SerializeField] private HandModelBase leftHand;
        [SerializeField] private HandModelBase rightHand;
        
        // helper vars
        public float trackedLeftRemaining;
        public float trackedRightRemaining;

        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            trackedLeftRemaining = minTrackedDuration;
            trackedRightRemaining = minTrackedDuration;
        }
        
        
        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
            trackedLeftRemaining = 0f;
            trackedRightRemaining = 0f;
        }

        private void Update()
        {
            if (base.stepState.Equals(StepState.StepStarted))
            {
                trackedLeftRemaining = leftHand.IsTracked ? (trackedLeftRemaining - Time.deltaTime) : minTrackedDuration;
                trackedRightRemaining = rightHand.IsTracked ? (trackedRightRemaining - Time.deltaTime) : minTrackedDuration;

                if (trackedLeftRemaining <= 0.0f && trackedRightRemaining <= 0.0f) {
                    FinishedCriteria = true;
                }
            }
        }
    }
}
