using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;

namespace DFKI.NMY
{
    public class HandTrackingLostDisabler : MonoBehaviour
    {
        [SerializeField] private HandModelBase left;
        [SerializeField] private HandModelBase right;

        [SerializeField] private List<GameObject> disableOnTrackingLostLeft;
        [SerializeField] private List<GameObject> disableOnTrackingLostRight;

        private bool lastStateLeft = true;
        private bool lastStateRight = true;
        
        void Update()
        {
	        if (lastStateLeft != left.IsTracked)
	        {

		        lastStateLeft = left.IsTracked;
		        foreach (var go in disableOnTrackingLostLeft) {
			        if (go) {
				        go.SetActive(left.IsTracked);
			        }
		        }
	        }

	        if (lastStateRight != right.IsTracked)
	        {
		        lastStateRight = right.IsTracked;
		        foreach (var go in disableOnTrackingLostRight)
		        {
			        if (go)
			        {
				        go.SetActive(left.IsTracked);
			        }
		        }

	        }

        }
    }
}
