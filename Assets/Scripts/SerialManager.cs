using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DFKI.NMY
{
    using System.Diagnostics;
    using System.IO;
    using global::NMY;

    public class SerialManager : StartupBehaviour
    {
        private Process serialInfoProc;
        private string[] data;

        public string deviceName;
        public string port;

        // Start is called before the first frame update
        protected override void StartupEnter()
        {
            GetSerialPort();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void GetSerialPort()
        {
            UnityEngine.Debug.Log("Try to get serial port info.");
            UnityEngine.Debug.Log("Starting SerialPortDataTester.exe...");
            ProcessStartInfo startInfoProc = new ProcessStartInfo();
            startInfoProc.FileName = Application.dataPath + @"/../SerialPortInfo/SerialPortDataTester.exe";
            UnityEngine.Debug.Log(startInfoProc.FileName);
            startInfoProc.WorkingDirectory = Application.dataPath + @"/../SerialPortInfo";
            serialInfoProc = Process.Start(startInfoProc);
            serialInfoProc.EnableRaisingEvents = true;
            serialInfoProc.Exited += SerialInfoProc_Exited;
        }

        private void SerialInfoProc_Exited(object sender, EventArgs e)
        {
            UnityEngine.Debug.Log("SerialPortDataTester.exe  finished. Reading data...");
            data = File.ReadAllLines(Application.dataPath + @"/../SerialPortInfo/serialInfo.txt");
            for (int i = 0; i < data.Length; i++)
            {
                UnityEngine.Debug.Log(data[i]);
                if (data[i].StartsWith(deviceName) || data[i].Contains(deviceName))
                {
                    string portNumber = data[i].Substring(data[i].IndexOf("(COM")).Trim();
                    port = portNumber.Substring(1, portNumber.Length - 2);
                    UnityEngine.Debug.Log($"{deviceName} found at port: {port}");

                    SerialController.instance.portName = port;
                    SerialController.instance.Init();

                    UnityEngine.Debug.Log(SerialController.instance.GetInstanceID());
                    break;
                }
            }
            
            serialInfoProc.Exited -= SerialInfoProc_Exited;
            UnityEngine.Debug.Log("Done.");
        }
    }
}
