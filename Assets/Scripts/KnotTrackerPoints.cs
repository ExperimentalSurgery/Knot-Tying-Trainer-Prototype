using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Globalization;

namespace DFKI.NMY
{
    public class KnotTrackerPoints : MonoBehaviour
    {
        private KnotTrackerManager knotTrackerManager;
        private bool isRecording;
        private Vector3 currentPos;
        private StringBuilder dataBuilder;
        private int currentFrame = 0;

        // Start is called before the first frame update
        void Start()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-UK");
            knotTrackerManager = GetComponent<KnotTrackerManager>();
        }

        public void StartRecording()
        {
            Debug.Log("Start Recording of Knot Points");
            currentFrame = 0;
            dataBuilder = new StringBuilder();
            dataBuilder.AppendLine("Frame,Point1_X,Point1_Y,Point1_Z,Point2_X,Point2_Y,Point2_Z,Point3_X,Point3_Y,Point3_Z,Point4_X,Point4_Y,Point4_Z,Point5_X,Point5_Y,Point5_Z,Point6_X,Point6_Y,Point6_Z,Point7_X,Point7_Y,Point7_Z,Point8_X,Point8_Y,Point8_Z,Point9_X,Point9_Y,Point9_Z,Point10_X,Point10_Y,Point10_Z");
            isRecording = true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                if (isRecording)
                    return;

                StartRecording();

            }
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                if (!isRecording)
                    return;
               
                SaveToDisk();
            }

            if (isRecording)
            {
                CreateEntry();
            }
        }

        public void CreateEntry()
        {
            if (!isRecording)
                return;

            currentFrame++;
            dataBuilder.Append(currentFrame.ToString() + ",");
            for (int i = 0; i < knotTrackerManager.blobObjects.Length; i++)
            {
                currentPos = knotTrackerManager.blobObjects[i].transform.position;
                dataBuilder.Append(currentPos.x + "," + currentPos.y + "," + currentPos.z);
                if(i < knotTrackerManager.blobObjects.Length-1)
                    dataBuilder.Append(",");
            }
            dataBuilder.Append("\n");
        }
        public void SaveToDisk()
        {
            currentFrame = 0;
            Debug.Log("Stop Recording of Knot Points, saving to: " + Application.dataPath + "/../KnotPointData/data.csv");
            isRecording = false;
            File.WriteAllText(Application.dataPath + "/../KnotPointData/data.csv", dataBuilder.ToString());
        }
    }
}
