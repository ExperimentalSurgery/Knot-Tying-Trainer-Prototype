using System;
using System.Collections;
using System.Collections.Generic;
using NMY;
using UnityEngine;
using UnityEngine.XR.Management;
using Varjo.XR;

namespace DFKI.NMY
{
    public class GreifbARVarjoManager : StartupBehaviour {
        [SerializeField] private MarkerManager markerManager;
        [SerializeField] private MixedRealityExample mixedRealityExample;
        
        protected override void StartupEnter() {
            
            
            if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
            {
                try
                {
                    var loader = XRGeneralSettings.Instance.Manager.activeLoader as Varjo.XR.VarjoLoader;
                    var cameraSubsystem = loader.cameraSubsystem as VarjoCameraSubsystem;
                    markerManager.gameObject.SetActive(true);
                    mixedRealityExample.gameObject.SetActive(true);
                }
                catch (NullReferenceException e)
                {
                    Debug.Log("No Varjo detected. Disable Varjo related Components "+e.Message);
                    markerManager.gameObject.SetActive(false);
                    mixedRealityExample.gameObject.SetActive(false);
            
                }

            }

        }

    }
}
