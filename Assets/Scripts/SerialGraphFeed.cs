using ChartAndGraph;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityAtoms.BaseAtoms;

namespace DFKI.NMY
{
    public class SerialGraphFeed : MonoBehaviour
    {
        public string dataFile;
        public int resolution;

        // Start is called before the first frame update
        void Start()
        {
            GraphChartBase graph = GetComponent<GraphChartBase>();
            if (graph != null)
            {
                graph.HorizontalValueToStringMap[0.0] = "Zero"; // example of how to set custom axis strings
                graph.DataSource.StartBatch();
                graph.DataSource.ClearAndMakeBezierCurve("Tension");
                graph.DataSource.ClearAndMakeLinear("Contact");

                if (File.Exists(dataFile))
                {
                    string[] data = File.ReadAllLines(dataFile);
                    for (int i = 0; i < data.Length; i+=(int)(resolution))
                    {
                        string[] entryPoints = data[i].Split(';');
                        
                        graph.DataSource.AddPointToCategory("Contact", i, int.Parse(entryPoints[0])*1000);

                        if (i == 0)
                            graph.DataSource.SetCurveInitialPoint("Tension", i, 0);
                        else
                            graph.DataSource.AddLinearCurveToCategory("Tension", new DoubleVector2(i, int.Parse(entryPoints[1])));
                    }
                    graph.DataSource.MakeCurveCategorySmooth("Tension");
                    graph.DataSource.EndBatch();
                }                
            }            
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
