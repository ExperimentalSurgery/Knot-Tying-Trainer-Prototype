using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DFKI.NMY
{
    public class CopyTransFromMarkerDummy : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            GameObject[] markerDummy = GameObject.FindGameObjectsWithTag("MarkerDummy");

            if (markerDummy != null)
            {
                if(markerDummy.Length > 0)
                {
                    transform.position = markerDummy[0].transform.position;
                    transform.rotation = markerDummy[0].transform.rotation;
                }
            }
        }
    }
}
