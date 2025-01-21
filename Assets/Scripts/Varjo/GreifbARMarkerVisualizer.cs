using UnityEngine;
using Varjo.XR;

namespace DFKI.NMY {
    public class GreifbARMarkerVisualizer : MonoBehaviour {
        
        public int markerID = 200;
        public long markerTimeout = 3000;
        public bool doPrediction = false;
        public bool disableMarkerManagerWhenTracked = false;


        public void UpdateSettings()
        {
            
           
            
            if (doPrediction) {
                VarjoMarkers.AddVarjoMarkerFlags(markerID, VarjoMarkerFlags.DoPrediction);
            }
            else
            {
                VarjoMarkers.SetVarjoMarkerTimeout(markerID, markerTimeout);
            }
        }
        public void UpdatePose(VarjoMarker marker) {
           
            transform.localPosition = marker.pose.position;
            transform.localRotation = marker.pose.rotation;
            
            /*ParentConstraint[] pcList = FindObjectsOfType<ParentConstraint>(true);
            for (int i = 0; i < pcList.Length; i++)
            {
               
                    childTransform = pcList[i];
                    ConstraintSource scr = new ConstraintSource();
                    scr.sourceTransform = this.transform;
                    scr.weight = 1f;
                    if(childTransform.sourceCount > 0)
                        childTransform.SetSource(0, scr);
                    else
                        childTransform.AddSource(scr);
                    childTransform.constraintActive = true;
                    childTransform.SetRotationOffset(0, new Vector3(0,180,0));

                }
            } 
            */
        }
    }
}
