using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor.Localization.Plugins.XLIFF.V20;
#endif

namespace DFKI.NMY
{
    public class SerialDataCollector : MonoBehaviour
    {
        public RawImage dataGraphImage;
        public int graphHeight;

        private int tmpInt;
        private int contactVal;
        public int tensionVal;
        private SerialController sc;
        private StringBuilder dataLogger;
        private Texture2D dataGraphTexture;
        
        

        // Start is called before the first frame update
        void Start()
        {
            Application.targetFrameRate = 90;
            sc = GetComponent<SerialController>();
            sc.SerialMessageEventHandler += OnSerialMessage;
            dataLogger = new StringBuilder();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        // Invoked when a line of data is received from the serial device.
        void OnSerialMessage(object sender, MessageEventArgs msg)
        {
            string[] data = msg.message.Split(';');

            if (int.TryParse(data[1], out tmpInt))
            {
                tensionVal = tmpInt;
            }

            if (int.TryParse(data[0], out tmpInt))
            {
                contactVal = tmpInt;
            }

            dataLogger.AppendLine(msg.message);
        }

        private void OnDisable()
        {
            sc.SerialMessageEventHandler -= OnSerialMessage;
            LogDataToFile();
        }

        [ContextMenu("LogDataToFile")]
        public void LogDataToFile()
        {
            File.WriteAllText(Application.dataPath + "/../SerialPortInfo/data.csv", dataLogger.ToString());
        }

        [ContextMenu("PlotGraph")]
        public void PlotGraph()
        {
            string[] data = File.ReadAllLines(Application.dataPath + "/../SerialPortInfo/data.csv");
            dataGraphTexture = new Texture2D(data.Length, graphHeight, TextureFormat.RGB24, false, false);
            dataGraphTexture.filterMode = FilterMode.Point;
            for (int i = 0; i < data.Length; i++)
            {
                string[] dataLine = data[i].Split(';');
                dataGraphTexture.SetPixel(i, (int)Map(int.Parse(dataLine[1]), 0,1000,0, graphHeight), Color.red);
            }
            dataGraphTexture.Apply();
            dataGraphImage.texture = dataGraphTexture;
        }

        [ContextMenu("LogDataAndPlot")]
        public void LogDataAndPlot()
        {
            LogDataToFile();
            PlotGraph();
        }

        float Map(float x, float in_min, float in_max, float out_min, float out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }
    }
}
