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

                var loader = XRGeneralSettings.Instance.Manager.activeLoader as Varjo.XR.VarjoLoader;
                var cameraSubsystem = loader.cameraSubsystem as VarjoCameraSubsystem;

                if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
                {

                    if (VarjoMarkers.IsVarjoMarkersEnabled())
                    {

                        // Option 1: Already tracked
                        List<VarjoMarker> markers = new List<VarjoMarker>();
                        VarjoMarkers.GetVarjoMarkers(out markers);

                        if (markers.Count > 0)
                        {
                            foreach (var marker in markers)
                            {
                                if (marker.id.Equals(markerID))
                                {

                                    FinishedCriteria = true;
                                    return;
                                }
                            }
                        }
                    }

                    // Option 2: Wait for upcoming events
                    markerManager = FindObjectOfType<MarkerManager>();
                    markerManager.MarkerDetectedEventHandler -= OnMarkerDetected;
                    markerManager.MarkerDetectedEventHandler += OnMarkerDetected;
                    markerManager.markersEnabled = true;
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
