using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace DFKI.NMY
{
    public class NMYXRGazeHoverEffect : MonoBehaviour
    {
        [SerializeField] private GameObject customReticle;
        public LayerMask raycastLayer; // Specify the layers that the ray should interact with
        public GameObject reticle;
        void Update()
        {
           
                RaycastHit hit;
                // Check if the ray hits a collider on the specified layer
                if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100.0f))
                {
                   reticle.gameObject.SetActive(true);
                    reticle.transform.position = hit.point;
                    reticle.transform.eulerAngles = hit.normal;
                }
                else
                {
                    reticle.gameObject.SetActive(false);
                }
            
        }

        
    }
}
