using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NMY;
using NMY.VirtualRealityTraining;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace DFKI.NMY
{
    public struct DataSetEntry
    {
        public double Time;
        //public GreifbarChapter Chapter;
        //public BaseTrainingStep Step;
        public string Chapter;
        public string Step;
        public float ChapterOptimalTime;
        public float ChapterUserTime;
        public int Tension;
        public float GestureDeviation;
        public int GestureMaxDeviation;

        public override String ToString()
        {
            return //$"{Time.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}," +
                   $"{Time.ToString()}," +
                   //$"{Chapter.ChapterTitle.GetLocalizedString()}," +
                   //$"{Step.gameObject.name}," +
                   $"{Chapter}," +
                   $"{Step}," +
                   $"{ChapterOptimalTime.ToString("0.0", CultureInfo.InvariantCulture)}," +
                   $"{ChapterUserTime.ToString("0.0", CultureInfo.InvariantCulture)}," +
                   $"{Tension.ToString("0.0", CultureInfo.InvariantCulture)}," +
                   $"{GestureDeviation.ToString("0.0", CultureInfo.InvariantCulture)}," +
                   $"{GestureMaxDeviation.ToString("0.0", CultureInfo.InvariantCulture)}";
        }

        public DataSetEntry(double time, string chapter, string step, float chapterOptimalTime, float chapterUserTime, int tension, float gestureDeviation, int gestureMaxDeviation)
        {
            Time = time;
            Chapter = chapter;
            Step = step;
            ChapterOptimalTime = chapterOptimalTime;
            ChapterUserTime = chapterUserTime;
            Tension = tension;
            GestureDeviation = gestureDeviation;
            GestureMaxDeviation = gestureMaxDeviation;
        }
    }
    public class GreifbarUserPerformanceManager : SingletonStartupBehaviour<GreifbarUserPerformanceManager>
    {
        [SerializeField] private TrainingPhase currentPhase;
        [SerializeField] private CompletionPanelData completionPanelData;
        [SerializeField] private KnotbankCtrl tensionCollector;
        [SerializeField] private float interval  = 1f;

        public TextAsset CsvFile;

        private readonly DFKI.GestureModule _leftGestureModule = new();
        private readonly DFKI.GestureModule _rightGestureModule = new();
        
        private static List<DataSetEntry> _dataCollection = new List<DataSetEntry>();
        public List<DataSetEntry> DataCollection { get =>_dataCollection;}

        private GreifbarChapter currentChapter;
        private BaseTrainingStep currentStep;

        // runtime vars
        private float _chapterTime;
        private float timeSinceLastCheck = 0.0f;
        private float currentDeviation = 0.0f;
        private float currentMaxDeviation = 0.0f;

        // static vars
        public const int timeCol = 0;
        public const int chapterNameCol = 1;
        public const int stepNameCol = 2;
        public const int chapterOptimalTimeCol = 3; // target time
        public const int chapterUserTimeCol = 4; // Time time the user has spent from chapterstart. Last value needs to be used
        public const int tensionCol = 5;
        public const int gestureDeviationCol = 6;
        public const int maxGestureDeviationCol = 7;
        public static int horizontalDataSize = 1000;
        private static float lastLoadedTime;

        private GreifbarGestureDataFeed _gestureFeed;
        private GreifbarTimeDataFeed _timeFeed;
        private GreifbarTensionDataFeed _tensionFeed;

        private void Awake()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            GetGraphs();
        }

        private void GetGraphs()
        {
            _gestureFeed = FindObjectOfType<GreifbarGestureDataFeed>();
            _timeFeed = FindObjectOfType<GreifbarTimeDataFeed>();
            _tensionFeed = FindObjectOfType<GreifbarTensionDataFeed>();
        }

        private void OnEnable() {
            BaseTrainingStep.OnStepChanged -= OnStepChanged;
            BaseTrainingStep.OnStepChanged += OnStepChanged;
            GetGraphs();
        }

        private void OnDisable() {
            BaseTrainingStep.OnStepChanged -= OnStepChanged;
        }

        private void OnStepChanged(object sender, BaseTrainingStepEventArgs e)
        {
            // New chapter?
            if (e.step is GreifbarChapter && (e.step as GreifbarChapter).AnalysisChapter) {
                
                currentChapter=e.step as GreifbarChapter;
                _chapterTime = 0.0f;
            }
            
            // Check if IKnotbAR Interface is implemented
            if (e.step is IKnotbAR) {
                currentStep = e.step;
            }
        }

        protected virtual void CollectData()
        {
            if (currentChapter == null || currentChapter.AnalysisChapter==false) return;

            string chapterTitle = "EmptyTitle";
            if (currentChapter.ChapterTitle.IsEmpty == false) {
                chapterTitle = currentChapter.ChapterTitle.GetLocalizedString();
            }
            
            // Collect Data
            DataSetEntry dataSet = new DataSetEntry();
            dataSet.Time = Time.realtimeSinceStartupAsDouble;
            dataSet.Chapter = chapterTitle;
            dataSet.Step = currentStep.gameObject.name;
            dataSet.ChapterOptimalTime = currentChapter.TargetTime;
            dataSet.ChapterUserTime = _chapterTime;
            dataSet.Tension = tensionCollector.GetTensionValue();
            dataSet.GestureDeviation = currentDeviation;
            dataSet.GestureMaxDeviation = 5;
            _dataCollection.Add(dataSet);
            
            // UPDATE DATASETS
            completionPanelData.KnotTechniqueValue = 0; //TODO: NOT IMPLEMENTED 
            
            if (currentChapter.Phase == TrainingPhase.L3_ZEITMESSUNG) {
                completionPanelData.TotalTimeSecValue = GetTotalTimeTracked();
            }
            else {
                completionPanelData.TotalTimeSecValue = currentChapter.TargetTime-1;
            }

            if (currentChapter.Phase == TrainingPhase.L2_KNOTENSPANNUNG) {
                completionPanelData.TensionValue = GetTensionValuePercentage();
                
            }
            else
            {
                completionPanelData.TensionValue = 100;
            }
            //Debug.Log(dataSet.ToString());            
        }

        [ContextMenu("Update Graphs")]
        public void UpdateGraphs()
        {
            GetGraphs();
            _tensionFeed.UpdateGraph();
            _timeFeed.UpdateGraph();
            _gestureFeed.UpdateGraph();
        }

        [ContextMenu("LogDataToFile")]
        public void LogDataToFile()
        {
            if (_dataCollection == null || _dataCollection.Count == 0)
                return;

            StringBuilder resultData = new StringBuilder();
            string header = "Time,Chapter,Step,ChapterOptimalTime,ChapterUserTime,Tension,GestureDeviation,GestureMaxDeviation";
            resultData.AppendLine(header);
            foreach (DataSetEntry entry in _dataCollection) {
                resultData.AppendLine(entry.ToString());
            }

            if(!Directory.Exists(Application.streamingAssetsPath + "/sDataCollector"))
                Directory.CreateDirectory(Application.streamingAssetsPath + "/sDataCollector");

            File.WriteAllText(Application.streamingAssetsPath + "/sDataCollector/data.csv", resultData.ToString());
        }

        public float GetTimeForChapter(string chapterTitle)
        {
            List<float> chapterTimes = new List<float>();
            chapterTimes.Add(0f);
            foreach (var entry in _dataCollection) {
                if (entry.Chapter == chapterTitle) {
                    chapterTimes.Add(entry.ChapterUserTime);
                }
            }
            return chapterTimes.Max();
        }

        public List<string> GetPlayedChapters()
        {
            List<string> chapters = new List<string>();
            foreach (var entry in _dataCollection) {
                if (chapters.Contains(entry.Chapter) == false) {
                    chapters.Add(entry.Chapter);
                }
            }

            return chapters;
        }
        
        public float GetTotalTimeTracked() {
            return (float)_dataCollection.Last().Time;
        }

        public float GetTensionValuePercentage()
        {
            if (_dataCollection.Count == 0) return 0;
            
            float minVal=50;
            float maxVal=200;

            int _positives = 0;
            foreach (var entry in _dataCollection) {
                if (entry.Tension >= minVal && entry.Tension <= maxVal) {
                    _positives++;
                }
            }
            
            float _ratio = _positives / (float)_dataCollection.Count;
            return _ratio * 100f;
        }


        public static List<DataSetEntry> SetInitialData(string dataText)
        {
            if (lastLoadedTime != 0 && lastLoadedTime - Time.realtimeSinceStartup < 2f)
            {
                Debug.Log("Data already loaded for this graph, skipping reload.");
                return null;
            }

            _dataCollection.Clear();

            if (!string.IsNullOrEmpty(dataText))
            {
                using (StringReader reader = new StringReader(dataText))
                {
                    string line;
                    double time = 0.0f;
                    // GreifbarChapter chapter = new GreifbarChapter();
                    // BaseTrainingStep step = new BaseTrainingStep();
                    string chapter = "chapter";
                    string step = "step";
                    float chapterOptimalTime = 0.0f;
                    float chapterUserTime = 0.0f;
                    int tension = 0;
                    float gestureDeviation = 0.0f;
                    int maxGestureDeviation = 0;

                    int currentLine = 0;
                    while ((line = reader.ReadLine()) != null)
                    {

                        // skip header
                        if (currentLine == 0)
                        {
                            currentLine++;
                            continue;
                        }
                        string[] parts = line.Split(',');

                        time = double.Parse(parts[timeCol]);
                        chapter = parts[chapterNameCol];
                        step = parts[stepNameCol];
                        chapterOptimalTime = float.Parse(parts[chapterOptimalTimeCol]);
                        chapterUserTime = float.Parse(parts[chapterUserTimeCol]);
                        tension = int.Parse(parts[tensionCol]);
                        gestureDeviation = float.Parse(parts[gestureDeviationCol]);
                        maxGestureDeviation = int.Parse(parts[maxGestureDeviationCol]);

                        _dataCollection.Add(new DataSetEntry(time, chapter, step, chapterOptimalTime, chapterUserTime, tension, gestureDeviation, maxGestureDeviation));
                        currentLine++;
                    }
                    horizontalDataSize = (int)(time + 1);
                }
                Debug.Log($"created data for {_dataCollection.Count} entries");
                lastLoadedTime = Time.realtimeSinceStartup;
                return _dataCollection;
            }
            else
            {
                for (int i = 0; i < 100000; i++)    // initialize with random data
                {
                    double x = i;
                    int y = (int)(UnityEngine.Random.value * 10f - 5f);
                    _dataCollection.Add(new DataSetEntry(i, "test chapter", "test step", 5, 5, y, 10, 5));
                }
                lastLoadedTime = Time.realtimeSinceStartup;
                return _dataCollection;
            }
        }

        public static List<DataSetEntry> LoadFromFile(string path)
        {
            string line;
            double time = 0.0f;
            string chapter = "chapter";
            string step = "step";
            float chapterOptimalTime = 0.0f;
            float chapterUserTime = 0.0f;
            int tension = 0;
            float gestureDeviation = 0.0f;
            int maxGestureDeviation = 0;

            _dataCollection.Clear();

            try
            {
                using (StreamReader file = new StreamReader(path))
                {
                    int currentLine = 0;
                    while ((line = file.ReadLine()) != null)
                    {

                        // skip header
                        if (currentLine == 0)
                        {
                            currentLine++;
                            continue;
                        }
                        string[] parts = line.Split(',');

                        time = double.Parse(parts[timeCol]);
                        chapter = parts[chapterNameCol];
                        step = parts[stepNameCol];
                        chapterOptimalTime = float.Parse(parts[chapterOptimalTimeCol]);
                        chapterUserTime = float.Parse(parts[chapterUserTimeCol]);
                        tension = int.Parse(parts[tensionCol]);
                        gestureDeviation = float.Parse(parts[gestureDeviationCol]);
                        maxGestureDeviation = int.Parse(parts[maxGestureDeviationCol]);

                        _dataCollection.Add(new DataSetEntry(time, chapter, step, chapterOptimalTime, chapterUserTime, tension, gestureDeviation, maxGestureDeviation));
                        currentLine++;
                    }
                }
                return _dataCollection;
            }
            catch
            {
                return null;
            }
        }

        private void Update()
        {
            if (currentChapter && currentChapter.AnalysisChapter == false) return;
            
            if (currentChapter && currentStep && (currentStep is IKnotbAR)) {
                _chapterTime += (currentStep as IKnotbAR).AffectTimer ? Time.deltaTime : 0;
                completionPanelData.Phase = currentChapter.Phase;
            }

            if (currentStep is KnotGestureStep)
            {
                currentDeviation = (_leftGestureModule.currentMeanDeviation  + _rightGestureModule.currentMeanDeviation ) * 0.5f;
                currentMaxDeviation = (currentStep as KnotGestureStep).CumulatedThreshold;
            }
            
            timeSinceLastCheck += Time.deltaTime;
            if (timeSinceLastCheck >= interval)
            {
                CollectData();
                timeSinceLastCheck = 0f; // Reset the timer
            }            
        }
    }
}
