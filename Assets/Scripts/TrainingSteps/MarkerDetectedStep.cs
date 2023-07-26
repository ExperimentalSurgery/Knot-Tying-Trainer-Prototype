using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Leap.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Management;
using Varjo;
using Varjo.XR;

namespace DFKI.NMY
{
    public class MarkerDetectedStep : GreifbarBaseStep
    {

        [SerializeField] private int markerID;
        [SerializeField] private MarkerManager markerManager;

        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);

                try
                {
                    markerManager = FindObjectOfType<MarkerManager>();
                    markerManager.MarkerDetectedEventHandler += OnMarkerDetected;
                }
                catch (Exception e)
                {
                    FinishedCriteria = true;
                }

        }
        
        
        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
            if (markerManager) {
                markerManager.MarkerDetectedEventHandler -= OnMarkerDetected;
            }
        }

        private void OnMarkerDetected(object sender, MarkerDetectedEventArgs e)
        {
            if (e.marker.id == markerID)
            {
                FinishedCriteria = true;
            }
        }
    }
}
