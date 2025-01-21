using System;
using System.Collections;
using System.Collections.Generic;
using NMY;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace DFKI.NMY
{
    public class GreifbARWorldSpaceButton : MonoBehaviour {

        [SerializeField] private XRSimpleInteractable interactable;
        [SerializeField] private SimpleAnimatorActivatable highlighter;
        [SerializeField] private float cooldown = 2.0f;
        private float remainingCooldown = 0;
        
        public XRSimpleInteractable Interactable
        {
            get => interactable;
            set => interactable = value;
        }

        private void OnEnable()
        {
            interactable.selectEntered.AddListener(OnSelect);
        }
        
        private void OnDisable()
        {
            interactable.selectEntered.RemoveListener(OnSelect);
        }

        private void OnSelect(SelectEnterEventArgs arg0)
        {
            remainingCooldown = cooldown;
            Debug.Log("XWorldSpace Button Selected " + gameObject.name);
            
        }

        public void Highlight(bool state) {
            highlighter.Activate(state);
        }

        private void Update()
        {
            
            if (remainingCooldown > 0)
            {
                remainingCooldown -= Time.deltaTime;
                foreach (var c in interactable.colliders){
                    //c.enabled = false;
                       
                }
            }
            else
            {
                foreach (var c in interactable.colliders){
                    //c.enabled = true;
                }
            }
        }
    }
}
