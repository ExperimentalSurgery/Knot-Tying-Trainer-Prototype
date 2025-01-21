using System;
using System.Linq;
using Leap.Unity;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DFKI.NMY
{
    public class UltraleapHandStatusHandler : MonoBehaviour
    {
        
        // Static Vars
        public static bool TrackedLeft;
        public static bool TrackedRight;
        
        // Static Events
        public static UnityEvent LeftTrackingCritical = new UnityEvent();
        public static UnityEvent LeftTrackingIsBack = new UnityEvent();
        public static UnityEvent RightTrackingCritical = new UnityEvent();
        public static UnityEvent RightTrackingIsBack = new UnityEvent();

        [Header("Events")]
        [SerializeField] private float sendWarningDuration = 5;
        [SerializeField] private float minTrackedDuration = 2;
        
        [Header("Left Events")]
        public UnityEvent leftTrackingCritical = new UnityEvent();
        public UnityEvent leftTrackingIsBack = new UnityEvent();
        
        [Header("Right Events")]
        public UnityEvent rightTrackingCritical = new UnityEvent();
        public UnityEvent rightTrackingIsBack = new UnityEvent();
        
        // privates 
        private float _notTrackedLeftDuration;
        private float _notTrackedRightDuration;
        private float _trackedLeftDuration;
        private float _trackedRightDuration;
        private HandModelBase _leftHandBaseModel;
        private HandModelBase _rightHandBaseModel;

        private void Awake() {
            
            // lets start with critical state and wait for tracking
            TrackedLeft = false;
            TrackedRight = false;
            
            
       
            
        }

     

        private void FindBaseModels() {
            
            var handModels = FindObjectsOfType<HandModelBase>(true).ToList();

            foreach (var handModel in handModels) {
                
                if(handModel.GetLeapHand()==null)continue;
                
                if (handModel.GetLeapHand().IsLeft) {
                    _leftHandBaseModel = handModel;
                }
                if (handModel.GetLeapHand().IsRight) {
                    _rightHandBaseModel = handModel;
                }
            }
            
        }

        private void FixedUpdate() {

            if (_leftHandBaseModel == null || _rightHandBaseModel == null) {
                FindBaseModels();
            }
            
            if (_leftHandBaseModel) {

                // Update warning times
                _notTrackedLeftDuration = _leftHandBaseModel.IsTracked==false ?_notTrackedLeftDuration+Time.deltaTime : 0.0f;
                _trackedLeftDuration = _leftHandBaseModel.IsTracked ? _trackedLeftDuration+Time.deltaTime : 0.0f;
                
                // Invoke Events
                if (TrackedLeft  && _notTrackedLeftDuration >= sendWarningDuration) {
                        InvokeLeftCritical();
                        TrackedLeft = false;
                    
                }
                
                if(TrackedLeft == false && _trackedLeftDuration >= minTrackedDuration) {
                    InvokeLeftIsBack();
                    TrackedLeft = true;
                }   
            }
            
            if (_rightHandBaseModel) {

                // Update warning times
                _notTrackedRightDuration = _rightHandBaseModel.IsTracked==false ?_notTrackedRightDuration+Time.deltaTime : 0.0f;
                _trackedRightDuration =_rightHandBaseModel.IsTracked ? _trackedRightDuration+Time.deltaTime : 0.0f;
                
                // Invoke Events
                if (TrackedRight && _notTrackedRightDuration >= sendWarningDuration) {
                    InvokeRightCritical();
                    TrackedRight = false;
                }
                
                if(TrackedRight == false && _trackedRightDuration >= minTrackedDuration){
                    InvokeRightIsBack();
                    TrackedRight = true;
                }
            }
            
        }
        
        private void InvokeLeftCritical() {
            LeftTrackingCritical.Invoke();
            leftTrackingCritical.Invoke();
        }

        private void InvokeRightCritical()
        {
            RightTrackingCritical.Invoke();
            rightTrackingCritical.Invoke();
        }

        private void InvokeLeftIsBack()
        {
            LeftTrackingIsBack.Invoke();
            leftTrackingIsBack.Invoke();
        }

        private void InvokeRightIsBack()
        {
            RightTrackingIsBack.Invoke();
            rightTrackingIsBack.Invoke();
        }

    }
}
