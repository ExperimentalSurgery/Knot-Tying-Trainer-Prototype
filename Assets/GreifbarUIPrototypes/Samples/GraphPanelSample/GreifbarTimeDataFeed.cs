#define Graph_And_Chart_PRO
using UnityEngine;
using ChartAndGraph;
using System.Collections.Generic;
using System;
using System.IO;
using DFKI.NMY;

public partial class GreifbarTimeDataFeed : MonoBehaviour, IComparer<DoubleVector2>
{
    
    [SerializeField] private GraphChartBase timeGraph;
    
    public string Category1 = "TimeGood";
    public string Category2 = "TimeBad";

    public int DownSampleToPoints = 100;
    double pageSize = 2f;
    double currentPagePosition = 0.0;
    double currentZoom = 0f;
    double mCurrentPageSizeFactor = double.NegativeInfinity;

    public bool LoadExample = true;
    public bool ControlViewPortion = false;
    double? mSlideEndThreshold = null;
    int mStart = 0;

    [HideInInspector]
    public GraphChartBase AlternativeGraph = null;

    public bool drawInUpdate;
    public bool hasDrawn { get; private set; }

    private string currentChapter;
    private float currentChapterOptimalTime;
    private float currentChapterUserTime;
    private string lastChapter;
    private bool lastUserTimeWasGood;

    void Start()
    {
        
        if (AlternativeGraph != null)
            timeGraph = AlternativeGraph;
        if (LoadExample)
            SetInitialData();
        else
        {
            SetData();
        }            
    }

    public DataSetEntry GetLastPoint()
    {
        if (GreifbarUserPerformanceManager.instance.DataCollection.Count == 0)
            return new DataSetEntry();
        return GreifbarUserPerformanceManager.instance.DataCollection[GreifbarUserPerformanceManager.instance.DataCollection.Count - 1];
    }
    
    /// <summary>
    /// called with Start(). These will be used to load the data into the large data feed. You should replace this with your own loading logic.
    /// </summary> 
    void SetInitialData()
    {
        GreifbarUserPerformanceManager.SetInitialData(GreifbarUserPerformanceManager.instance.CsvFile.text);
    }

    public void SaveToFile(string path)
    {
        using (StreamWriter file = new StreamWriter(path))
        {
            file.WriteLine(GreifbarUserPerformanceManager.instance.DataCollection.Count);
            for (int i = 0; i < GreifbarUserPerformanceManager.instance.DataCollection.Count; i++)
            {
                DataSetEntry item = GreifbarUserPerformanceManager.instance.DataCollection[i];
                file.WriteLine(item.Time.ToString(), item.Chapter, item.Step, item.ChapterOptimalTime, item.ChapterUserTime, item.Tension, item.GestureDeviation, item.GestureMaxDeviation);
            }
        }
    }
    public void LoadFromFile(string path)
    {
        GreifbarUserPerformanceManager.LoadFromFile(path);
        SetData();
    }

    /// <summary>
    /// vertify's that the graph data is sorted so it can be searched using a binary search.
    /// </summary>
    /// <returns></returns>
    bool VerifySorted(List<DataSetEntry> data)
    {
        if (data == null)
            return true;
        for (int i = 1; i < data.Count; i++)
        {
            if (data[i].Time < data[i - 1].Time)
                return false;
        }
        return true;
    }

    partial void OnDataLoaded();
    /// <summary>
    /// set the data of the large data graph
    /// </summary>
    public void SetData()
    {
        if (GreifbarUserPerformanceManager.instance.DataCollection == null)
        {
            Debug.LogWarning("The data was not initialized correctly, aborting operation");
            return;
        }
        if (VerifySorted(GreifbarUserPerformanceManager.instance.DataCollection) == false)
        {
            Debug.LogWarning("The data used with large data feed must be sorted acoording to the x value, aborting operation");
            return;
        }
        OnDataLoaded();
        LoadPage(currentPagePosition); // load the page at position 0
    }

    //int FindClosestIndex(double position) // if you want to know what is index is currently displayed . use binary search to find it
    //{
    //    //NOTE :: this method assumes your data is sorted !!! 
    //    int res = mData.BinarySearch(new DoubleVector2(position, 0.0), this);
    //    if (res >= 0)
    //        return res;
    //    return ~res;
    //}

    double PageSizeFactor
    {
        get
        {
            return pageSize * timeGraph.DataSource.HorizontalViewSize;
        }
    }

    void AdjustHorizontalView()
    {
        int start, end;
        findAdjustPosition(timeGraph.HorizontalScrolling, timeGraph.DataSource.HorizontalViewSize, out start, out end);
        double minX = double.MaxValue, maxX = double.MinValue;

        bool show = timeGraph.AutoScrollHorizontally;
        if (GreifbarUserPerformanceManager.instance.DataCollection.Count == 0)
            show = true;
        else
        {
            double viewX = GreifbarUserPerformanceManager.instance.DataCollection[GreifbarUserPerformanceManager.instance.DataCollection.Count - 1].Time;
            double pageStartThreshold = currentPagePosition - mCurrentPageSizeFactor;
            double pageEndThreshold = currentPagePosition + mCurrentPageSizeFactor - timeGraph.DataSource.HorizontalViewSize;
            if (viewX >= pageStartThreshold && viewX <= pageEndThreshold)
                show = true;
        }
        if (show)
            --end;

        end = GreifbarUserPerformanceManager.instance.DataCollection.Count - 1;

        //Debug.Log("AdjustHorizontalView");

        for (int i = start; i <= end; i++)
        {
            double x = GreifbarUserPerformanceManager.instance.DataCollection[i].Time;
            minX = Math.Min(x, minX);
            maxX = Math.Max(x, maxX);
        }

        if (show)
        {
            DoubleVector3 p;
            if (timeGraph.DataSource.GetLastPoint(Category1, out p))
            {
                minX = Math.Min(p.x, minX); //TODO: ADD Offset
                maxX = Math.Max(p.x, maxX);  //TODO: ADD Offset
            }
        }
        timeGraph.HorizontalScrolling = minX;
        timeGraph.DataSource.HorizontalViewSize = maxX - minX;
    }

    void findAdjustPosition(double position,double size,out int start,out int end)
    {
        //int index = FindClosestIndex(position); // use binary search to find the closest position to the current scroll point
        int index = 0;
        double endPosition = position + size;

        start = index;

        for (end = index; end < GreifbarUserPerformanceManager.instance.DataCollection.Count; end++)
        {
            if (GreifbarUserPerformanceManager.instance.DataCollection[end].Time > endPosition) // take the first point that is out of the page
                break;
        }

    }
    void findPointsForPage(double position, out int start, out int end) // given a page position , find the right most and left most indices in the data for that page. 
    {
        //int index = FindClosestIndex(position); // use binary search to find the closest position to the current scroll point
        int index = 0;
        double endPosition = position + PageSizeFactor;
        double startPosition = position - PageSizeFactor;

        //starting from the current index , we find the page boundries
        for (start = index; start > 0; start--)
        {
            if (GreifbarUserPerformanceManager.instance.DataCollection[start].Time < startPosition) // take the first point that is out of the page. so the graph doesn't break at the edge
                break;
        }

        for (end = index; end < GreifbarUserPerformanceManager.instance.DataCollection.Count; end++)
        {
            if (GreifbarUserPerformanceManager.instance.DataCollection[end].Time > endPosition) // take the first point that is out of the page
                break;
        }
    }

    public void Update()
    {
        if(drawInUpdate)
        {
            UpdateGraph();
        }
    }

    public void UpdateGraph()
    {
        if (timeGraph != null)
        {
            //check the scrolling position of the graph. if we are past the view size , load a new page
            double pageStartThreshold = currentPagePosition - mCurrentPageSizeFactor;
            double pageEnd = currentPagePosition + mCurrentPageSizeFactor;
            if (mSlideEndThreshold.HasValue)
                pageEnd = Math.Max(mSlideEndThreshold.Value, pageEnd);
            double pageEndThreshold = pageEnd - timeGraph.DataSource.HorizontalViewSize * 1.0001;
            if (timeGraph.HorizontalScrolling < pageStartThreshold || timeGraph.HorizontalScrolling > pageEndThreshold || currentZoom >= timeGraph.DataSource.HorizontalViewSize * 2f)
            {
                currentZoom = timeGraph.DataSource.HorizontalViewSize;
                mCurrentPageSizeFactor = PageSizeFactor * 0.9f;
            }
            LoadPage(timeGraph.HorizontalScrolling);
            if (ControlViewPortion)
            {
                AdjustHorizontalView();
            }
        }
    }

    void LoadWithoutDownSampling(int start, int end)
    {
        //for (int i = start; i < end; i++) // load the data
        //{
        //    timeGraph.DataSource.AddPointToCategory(Category, GreifbarUserPerformanceManager.instance.DataCollection[i].Time, GreifbarUserPerformanceManager.instance.DataCollection[i].Tension);
        //}
    }

    void LoadWithDownSampling(int start, int end)
    {
        int total = end - start;

        if (DownSampleToPoints >= total)
        {
            LoadWithoutDownSampling(start, end);
            return;
        }

        double sampleCount = ((double)total) / (double)DownSampleToPoints;
        // graph.DataSource.AddPointToCategory(Category, mData[start].x, mData[start].y);
        for (int i = 0; i < DownSampleToPoints; i++)
        {
            int fractionStart = start + (int)(i * sampleCount); // the first point with a fraction
            int fractionEnd = start + (int)((i + 1) * sampleCount); // the first point with a fraction
            fractionEnd = Math.Min(fractionEnd, GreifbarUserPerformanceManager.instance.DataCollection.Count - 1);
            double x = 0, y = 0;
            double divide = 0.0;
            for (int j = fractionStart; j < fractionEnd; j++)  // avarge the points
            {
                x += GreifbarUserPerformanceManager.instance.DataCollection[j].Time;
                y += GreifbarUserPerformanceManager.instance.DataCollection[j].Tension;
                divide++;
            }
            if (divide > 0.0)
            {
                x /= divide;
                y /= divide;
                //timeGraph.DataSource.AddPointToCategory(Category, x, y);
            }
            else
                Debug.Log("error");
        }
        //   graph.DataSource.AddPointToCategory(Category, mData[last].x, mData[last].y);
    }

    public int GetIndex(int inGraphIndex)
    {
        return mStart + inGraphIndex;
    }

    void LoadPage(double pagePosition)
    {
        mSlideEndThreshold = null;
        if (timeGraph != null && GreifbarUserPerformanceManager.instance.DataCollection.Count > 0)
        {
            //Debug.Log("Loading page :" + pagePosition);
            timeGraph.DataSource.StartBatch(); // call start batch 
            timeGraph.DataSource.HorizontalViewOrigin = 0;
            int start, end;
            findPointsForPage(pagePosition, out start, out end); // get the page edges
            timeGraph.DataSource.ClearCategory(Category1); // clear the cateogry
            timeGraph.DataSource.ClearCategory(Category2); // clear the cateogry
            mStart = start;
            //if (DownSampleToPoints <= 0)
            //    LoadWithoutDownSampling(start, end);
            //else
            //    LoadWithDownSampling(start, end);

            //StringBuilder sb = new StringBuilder();
            for (int i = GreifbarUserPerformanceManager.instance.DataCollection.Count-1; i >= 0 ; i--)
            {
                if (currentChapter == null || GreifbarUserPerformanceManager.instance.DataCollection[i].Chapter != lastChapter)
                {
                    currentChapter = GreifbarUserPerformanceManager.instance.DataCollection[i].Chapter;
                    currentChapterOptimalTime = GreifbarUserPerformanceManager.instance.DataCollection[i].ChapterOptimalTime;
                    currentChapterUserTime = GreifbarUserPerformanceManager.instance.DataCollection[i].ChapterUserTime;

                    //sb.AppendLine($"Fill Graph with points at chapter: {currentChapter}");

                    if (i == GreifbarUserPerformanceManager.instance.DataCollection.Count - 1)
                    {
                        if (currentChapterUserTime > currentChapterOptimalTime)
                        {
                            timeGraph.DataSource.AddPointToCategory(Category1, GreifbarUserPerformanceManager.instance.DataCollection[i].Time, 0);
                            timeGraph.DataSource.AddPointToCategory(Category2, GreifbarUserPerformanceManager.instance.DataCollection[i].Time, 1);

                            lastUserTimeWasGood = false;

                            //sb.AppendLine($"negative at time: {GreifbarUserPerformanceManager.instance.DataCollection[i].Time} usertime: {currentChapterUserTime} ");
                        }
                        else
                        {
                            timeGraph.DataSource.AddPointToCategory(Category1, GreifbarUserPerformanceManager.instance.DataCollection[i].Time, 1);
                            timeGraph.DataSource.AddPointToCategory(Category2, GreifbarUserPerformanceManager.instance.DataCollection[i].Time, 0);

                            lastUserTimeWasGood = true;

                            //sb.AppendLine($"positive at time: {GreifbarUserPerformanceManager.instance.DataCollection[i].Time} usertime: {currentChapterUserTime}");
                        }
                    }
                    else
                    {
                        if (currentChapterUserTime > currentChapterOptimalTime)
                        {
                            timeGraph.DataSource.AddPointToCategory(Category1, GreifbarUserPerformanceManager.instance.DataCollection[i].Time, 0);
                            timeGraph.DataSource.AddPointToCategory(Category2, GreifbarUserPerformanceManager.instance.DataCollection[i].Time, 1);

                            if (lastUserTimeWasGood)
                            {
                                timeGraph.DataSource.AddPointToCategory(Category1, GreifbarUserPerformanceManager.instance.DataCollection[i].Time + 0.001f, 1);
                                timeGraph.DataSource.AddPointToCategory(Category2, GreifbarUserPerformanceManager.instance.DataCollection[i].Time + 0.001f, 0);
                            }
                            else
                            {
                                timeGraph.DataSource.AddPointToCategory(Category1, GreifbarUserPerformanceManager.instance.DataCollection[i].Time + 0.001f, 0);
                                timeGraph.DataSource.AddPointToCategory(Category2, GreifbarUserPerformanceManager.instance.DataCollection[i].Time + 0.001f, 1);
                            }

                            lastUserTimeWasGood = false;
                            //sb.AppendLine($"negative at time: {GreifbarUserPerformanceManager.instance.DataCollection[i].Time} usertime: {currentChapterUserTime} ");
                        }
                        else
                        {
                            timeGraph.DataSource.AddPointToCategory(Category1, GreifbarUserPerformanceManager.instance.DataCollection[i].Time, 1);
                            timeGraph.DataSource.AddPointToCategory(Category2, GreifbarUserPerformanceManager.instance.DataCollection[i].Time, 0);

                            if (lastUserTimeWasGood)
                            {
                                timeGraph.DataSource.AddPointToCategory(Category1, GreifbarUserPerformanceManager.instance.DataCollection[i].Time + 0.001f, 1);
                                timeGraph.DataSource.AddPointToCategory(Category2, GreifbarUserPerformanceManager.instance.DataCollection[i].Time + 0.001f, 0);
                            }
                            else
                            {
                                timeGraph.DataSource.AddPointToCategory(Category1, GreifbarUserPerformanceManager.instance.DataCollection[i].Time + 0.001f, 0);
                                timeGraph.DataSource.AddPointToCategory(Category2, GreifbarUserPerformanceManager.instance.DataCollection[i].Time + 0.001f, 1);
                            }

                            lastUserTimeWasGood = true;
                            //sb.AppendLine($"positive at time: {GreifbarUserPerformanceManager.instance.DataCollection[i].Time} usertime: {currentChapterUserTime}");
                        }
                    }
                }
                if (i == 0)
                {
                    if (currentChapterUserTime > currentChapterOptimalTime)
                    {
                        timeGraph.DataSource.AddPointToCategory(Category1, GreifbarUserPerformanceManager.instance.DataCollection[i].Time, 0);
                        timeGraph.DataSource.AddPointToCategory(Category2, GreifbarUserPerformanceManager.instance.DataCollection[i].Time, 1);
                        //sb.AppendLine($"negative at time: {GreifbarUserPerformanceManager.instance.DataCollection[i].Time} usertime: {currentChapterUserTime} ");
                    }
                    else
                    {
                        timeGraph.DataSource.AddPointToCategory(Category1, GreifbarUserPerformanceManager.instance.DataCollection[i].Time, 1);
                        timeGraph.DataSource.AddPointToCategory(Category2, GreifbarUserPerformanceManager.instance.DataCollection[i].Time, 0);
                        //sb.AppendLine($"positive at time: {GreifbarUserPerformanceManager.instance.DataCollection[i].Time} usertime: {currentChapterUserTime}");
                    }
                }
                lastChapter = currentChapter;
            }

            //if (sb.Length > 0)
            //{
            //    sb.AppendLine($".done.");
            //    Debug.Log(sb.ToString());
            //    sb.Clear();
            //}

            timeGraph.DataSource.EndBatch();
            timeGraph.HorizontalScrolling = pagePosition;
        }
        currentPagePosition = pagePosition;
    }


    public int Compare(DoubleVector2 x, DoubleVector2 y)
    {
        if (x.x < y.x)
            return -1;
        if (x.x > y.x)
            return 1;
        return 0;
    }
}

