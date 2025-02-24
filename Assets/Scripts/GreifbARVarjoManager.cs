using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Management;
using Varjo.XR;

namespace DFKI.NMY
{
    
    /// <summary>
    /// Component calling some Varjo-Related lines which will cause a Exception when no Varjo System is running.
    /// This error is used as detection mechanism and firing a event as well.
    /// Furthermore it only activates specific Gameobjects (f.e MarkerManager, MixedReality,...) when necassary. 
    /// </summary>
    public class GreifbARVarjoManager : MonoBehaviour
    {
        
        public List<GameObject> enableWhenVarjoSystem = new List<GameObject>();
        public List<GameObject> enableWhenOtherSystem = new List<GameObject>();
     
        public bool isVarjoSystem { private set; get; }
        
        [Header("Events")]
        // Event fires true => Varjo Detected, false => No Varjo
        public UnityEvent<bool> VarjoSystemChanged = new UnityEvent<bool>();


        protected void Awake(){
            
            isVarjoSystem = false;
            if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null) {
                try {
                    var loader = XRGeneralSettings.Instance.Manager.activeLoader as Varjo.XR.VarjoLoader;
                    var cameraSubsystem = loader.cameraSubsystem as VarjoCameraSubsystem;
                    isVarjoSystem = true;
                    UpdateGameObjectStates();
                    VarjoSystemChanged.Invoke(true);
                }
                catch (NullReferenceException e) {
                    Debug.LogWarning("No Varjo detected. Disable Varjo related Components "+e.Message);
                    isVarjoSystem = false;
                    UpdateGameObjectStates();
                    VarjoSystemChanged.Invoke(false);
                }
            }
        }

        private void Update()
        {

           
            
        }

        public void UpdateGameObjectStates()
        {
            foreach (GameObject v in enableWhenVarjoSystem) {
                v.SetActive(isVarjoSystem);
            }
            foreach (GameObject v in enableWhenOtherSystem) {
                v.SetActive(!isVarjoSystem);
            }

        }
    }
}
