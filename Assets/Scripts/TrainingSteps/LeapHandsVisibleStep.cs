using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Leap.Unity;
using NMY;
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
        [SerializeField] private ActivatableStartupBehaviour trackedHandsPreview;
        
        // helper vars
        private float trackedLeftRemaining;
        private float trackedRightRemaining;

        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            trackedLeftRemaining = minTrackedDuration;
            trackedRightRemaining = minTrackedDuration;
            if (trackedHandsPreview) {
                trackedHandsPreview.Activate();
            }
        }
        
        
        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
            trackedLeftRemaining = 0f;
            trackedRightRemaining = 0f;
            if (trackedHandsPreview) {
                trackedHandsPreview.Deactivate();
            }
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
