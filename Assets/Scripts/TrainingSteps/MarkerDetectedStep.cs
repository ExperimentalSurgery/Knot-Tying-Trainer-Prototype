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
    public class MarkerDetectedStep : GreifbarBaseStep,IKnotbAR
    {

        [SerializeField] private int markerID;

        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);

            try
            {
                
                var loader = XRGeneralSettings.Instance.Manager.activeLoader as Varjo.XR.VarjoLoader;
                var cameraSubsystem = loader.cameraSubsystem as VarjoCameraSubsystem;

                if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
                {
                    if (VarjoMarkers.IsVarjoMarkersEnabled())
                    {

                        // Option 1: Already tracked
                        if (GreifbARApp.instance.markerManager.IsMarkerTracked(markerID))
                        {
                            FinishedCriteria = true;
                        }
                    }

                    // Option 2: Wait for upcoming events
                    GreifbARApp.instance.markerManager = FindObjectOfType<GreifbARMarkerManager>();
                    GreifbARApp.instance.markerManager.markerDetected.AddListener(OnMarkerDetected);
                    GreifbARApp.instance.markerManager.markersEnabled = true;
                }
                else
                {
                    FinishedCriteria = true;
                }
            }
            catch (Exception e){
                Debug.Log("Skipping MarkerStep. Probably running the app without varjo connected is causing this...");
                Debug.Log(e.Message.ToString());
                FinishedCriteria = true;
                }

            
            
        }
        
        
        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
            if (GreifbARApp.instance.markerManager) {
                GreifbARApp.instance.markerManager.markerDetected.RemoveListener(OnMarkerDetected);
            }
        }

        private void OnMarkerDetected(MarkerDetectedEventArgs e)
        {
            if (e.marker.id == markerID)
            {
                FinishedCriteria = true;
            }
        }
    }
}
