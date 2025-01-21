using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Varjo.XR;

namespace DFKI.NMY
{
    public class GreifbARMarkerManager : MonoBehaviour
    {

        [SerializeField] private List<GreifbARMarkerVisualizer> preSpawnedMarkers;

        private List<VarjoMarker> markers;
        private List<long> markerIds;
        private List<long> absentIds;
        public Dictionary<long, GreifbARMarkerVisualizer> markerVisualizers;

        public Transform xrRig;

        public GameObject defaultPrefab;

        public MarkerObject[] markerObjects;

        public bool markersEnabled = true;
        private bool _markersEnabled;

        public UnityEvent<MarkerDetectedEventArgs> markerDetected = new UnityEvent<MarkerDetectedEventArgs>();
        public KeyCode enableMarkerManagerShortcut = KeyCode.M;





        [System.Serializable]
        public class MarkerObject
        {
            public int id;
            public GameObject prefab;
            public bool doPrediction = false;
            public long markerTimeout = 3000;
        }

        private void Awake()
        {
            markerDetected = new UnityEvent<MarkerDetectedEventArgs>();
            var found = FindObjectsOfType<GreifbARMarkerVisualizer>(true);
            foreach (var f in found)
            {
                if (preSpawnedMarkers.Contains(f)) continue;
                else preSpawnedMarkers.Add(f);
            }

        }

        protected virtual void Start()
        {
            markers = new List<VarjoMarker>();
            markerIds = new List<long>();
            absentIds = new List<long>();
            markerVisualizers = new Dictionary<long, GreifbARMarkerVisualizer>();


        }



        void Update()
        {

        if (Input.GetKeyDown(enableMarkerManagerShortcut))
        {
            markersEnabled = true;

        }


        if (markersEnabled != _markersEnabled) {
                markersEnabled = VarjoMarkers.EnableVarjoMarkers(markersEnabled);
                _markersEnabled = markersEnabled;
            }

            if (VarjoMarkers.IsVarjoMarkersEnabled())
            {
                markers.Clear();
                markerIds.Clear();
                VarjoMarkers.GetVarjoMarkers(out markers);
                if (markers.Count > 0)
                {
                    foreach (var marker in markers)
                    {
                        markerIds.Add(marker.id);
                        
                        if (markerVisualizers.ContainsKey(marker.id)) {
                            UpdateMarkerVisualizer(marker);
                        }
                        else if(FindPrefabMarkerObject(marker.id)!=null) {
                            Debug.Log("Create Marker Visualizer for id="+marker.id);
                            CreateMarkerVisualizerByPrefab(marker);
                        }
                        else if(FindPreSpawnedMarkerVisualizer(marker.id))
                        {
                            ConnectPrespawnedMarker(marker);
                        }

                        if (markerVisualizers.ContainsKey(marker.id)){

                            markersEnabled = false;

                        }

                    }
                }

                VarjoMarkers.GetRemovedVarjoMarkerIds(out absentIds);
                
                foreach (var id in absentIds)
                {
                    if (markerVisualizers.ContainsKey(id))
                    {
                        //Destroy(markerVisualizers[id].gameObject);
                        markerVisualizers[id].gameObject.SetActive(false);
                        markerVisualizers.Remove(id);
                    }

                    markerIds.Remove(id);
                }

                absentIds.Clear();
            }

            if (markerIds.Count == 0 && markerVisualizers.Count > 0)
            {
                var ids = markerVisualizers.Keys.ToArray();
                foreach (var id in ids)
                {
                    //Destroy(markerVisualizers[id].gameObject);
                    markerVisualizers[id].gameObject.SetActive(false);
                    markerVisualizers.Remove(id);
                }
            }
        }

        public bool IsMarkerTracked(long id)
        {
            return markerIds.Contains(id);
        }

        GreifbARMarkerVisualizer FindPreSpawnedMarkerVisualizer(long id)
        {
            
            var query = preSpawnedMarkers.Where(x => x.markerID == id);
            if (query.Count() > 0) {
                GreifbARMarkerVisualizer mo = query.ElementAt(0);
                return mo;
            }

            return null;
        }

        MarkerObject FindPrefabMarkerObject(long id)
        {
            var query = markerObjects.Where(x => x.id == id);

            if (query.Count() > 0)
            {
                MarkerObject mo = query.ElementAt(0);
                return mo;
            }

            return null;
        }

        void CreateMarkerVisualizerByPrefab(VarjoMarker marker) {
            
            Debug.Log("Create Marker for " + marker.id);
            MarkerObject markerContainer = FindPrefabMarkerObject(marker.id);
            GameObject objectToSpawn = markerContainer != null ? markerContainer.prefab :defaultPrefab;
            Transform markerTransform = Instantiate(objectToSpawn).transform;
            markerTransform.gameObject.name = $"{marker.id.ToString()} - {markerTransform.gameObject.name}";
            markerTransform.SetParent(xrRig);
            GreifbARMarkerVisualizer visualizer = markerTransform.gameObject.GetComponent<GreifbARMarkerVisualizer>();
            markerVisualizers.Add(marker.id, visualizer);
            visualizer.doPrediction = markerContainer.doPrediction;
            visualizer.markerTimeout = markerContainer.markerTimeout;
            visualizer.UpdateSettings();
            markerTransform.gameObject.SetActive(true);
            markerDetected.Invoke(new MarkerDetectedEventArgs(marker));

        }
        
        void ConnectPrespawnedMarker(VarjoMarker marker) {
            //Debug.Log("ConnectPrespawnedMarker "+marker.id);
            GreifbARMarkerVisualizer visualizerInScene = FindPreSpawnedMarkerVisualizer(marker.id);
            if (visualizerInScene == null) return;
            Transform markerTransform = visualizerInScene.transform;
            markerTransform.gameObject.name = $"{marker.id.ToString()} - {markerTransform.gameObject.name}";
            markerTransform.SetParent(xrRig);
            markerVisualizers.Add(marker.id, visualizerInScene);
            visualizerInScene.UpdatePose(marker);
            visualizerInScene.UpdateSettings();
            if (markerTransform)
            {
                markerTransform.gameObject.SetActive(true);
            }

            markerDetected.Invoke(new MarkerDetectedEventArgs(marker));
        }

        void UpdateMarkerVisualizer(VarjoMarker marker)
        {
            markerVisualizers[marker.id].UpdateSettings();
            markerVisualizers[marker.id].UpdatePose(marker);
        }


    }


}