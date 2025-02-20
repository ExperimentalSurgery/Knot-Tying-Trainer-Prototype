#define Graph_And_Chart_PRO
using UnityEngine;
using ChartAndGraph;
using System.Collections.Generic;
using System;
using System.IO;
using DFKI.NMY;

public partial class GreifbarTensionDataFeed : MonoBehaviour, IComparer<DoubleVector2>
{
    
    [SerializeField] private GraphChartBase tensionGraph;
    
    public string Category = "Tension";

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

    void Start()
    {
        
        if (AlternativeGraph != null)
            tensionGraph = AlternativeGraph;
        if (LoadExample)
            SetInitialData();
        else
        {
            //SetInitialData();
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

        // List<DoubleVector2> data = new List<DoubleVector2>(250000);
        // double x = 0f;
        // double y = 200f;
        // for (int i = 0; i < 25000; i++)    // initialize with random data
        // {
        //     data.Add(new DoubleVector2(x, y));
        //     y += UnityEngine.Random.value * 10f - 5f;
        //     x += UnityEngine.Random.value;
        // }
        // SetData(data);
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
            return pageSize * tensionGraph.DataSource.HorizontalViewSize;
        }
    }

    [Header("Offets x: min, y: max")]
    [Tooltip("x: MIN, y: MAX")]
    public Vector2 horizontalOffset;
    [Tooltip("x: MIN, y: MAX")]
    public Vector2 verticalOffset;

    void AdjustVerticalView()
    {
        int start, end;
        findAdjustPosition(tensionGraph.HorizontalScrolling,tensionGraph.DataSource.HorizontalViewSize,out start,out end);
        double minY = double.MaxValue, maxY = double.MinValue;

        bool show = tensionGraph.AutoScrollHorizontally;
        if (GreifbarUserPerformanceManager.instance.DataCollection.Count == 0)
            show = true;
        else
        {
            double viewX = GreifbarUserPerformanceManager.instance.DataCollection[GreifbarUserPerformanceManager.instance.DataCollection.Count - 1].Time;
            double pageStartThreshold = currentPagePosition - mCurrentPageSizeFactor;
            double pageEndThreshold = currentPagePosition + mCurrentPageSizeFactor - tensionGraph.DataSource.HorizontalViewSize;
            if (viewX >= pageStartThreshold && viewX <= pageEndThreshold)
                show = true;
        }
        if (show)
            --end;

        end = GreifbarUserPerformanceManager.instance.DataCollection.Count -1;

        for (int i=start; i<=end; i++)
        {
            double y = GreifbarUserPerformanceManager.instance.DataCollection[i].Tension;
            minY = Math.Min(y, minY);
            maxY = Math.Max(y, maxY);
        }

        if(show)
        {
            DoubleVector3 p;
            if (tensionGraph.DataSource.GetLastPoint(Category, out p))
            {
                minY = Math.Min(p.y, minY - verticalOffset.x) ; //TODO: ADD Offset
                maxY = Math.Max(p.y, maxY + verticalOffset.y) ;  //TODO: ADD Offset
            }
        }
        tensionGraph.VerticalScrolling = minY;
        tensionGraph.DataSource.VerticalViewSize = maxY - minY;
    }

    void AdjustHorizontalView()
    {
        int start, end;
        findAdjustPosition(tensionGraph.HorizontalScrolling, tensionGraph.DataSource.HorizontalViewSize, out start, out end);
        double minX = double.MaxValue, maxX = double.MinValue;

        bool show = tensionGraph.AutoScrollHorizontally;
        if (GreifbarUserPerformanceManager.instance.DataCollection.Count == 0)
            show = true;
        else
        {
            double viewX = GreifbarUserPerformanceManager.instance.DataCollection[GreifbarUserPerformanceManager.instance.DataCollection.Count - 1].Time;
            double pageStartThreshold = currentPagePosition - mCurrentPageSizeFactor;
            double pageEndThreshold = currentPagePosition + mCurrentPageSizeFactor - tensionGraph.DataSource.HorizontalViewSize;
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
            if (tensionGraph.DataSource.GetLastPoint(Category, out p))
            {
                minX = Math.Min(p.x, minX - horizontalOffset.x); //TODO: ADD Offset
                maxX = Math.Max(p.x, maxX + horizontalOffset.y);  //TODO: ADD Offset
            }
        }
        tensionGraph.HorizontalScrolling = minX;
        tensionGraph.DataSource.HorizontalViewSize = maxX - minX;
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
        if (tensionGraph != null)
        {
            //check the scrolling position of the graph. if we are past the view size , load a new page
            double pageStartThreshold = currentPagePosition - mCurrentPageSizeFactor;
            double pageEnd = currentPagePosition + mCurrentPageSizeFactor;
            if (mSlideEndThreshold.HasValue)
                pageEnd = Math.Max(mSlideEndThreshold.Value, pageEnd);
            double pageEndThreshold = pageEnd - tensionGraph.DataSource.HorizontalViewSize * 1.0001;
            if (tensionGraph.HorizontalScrolling < pageStartThreshold || tensionGraph.HorizontalScrolling > pageEndThreshold || currentZoom >= tensionGraph.DataSource.HorizontalViewSize * 2f)
            {
                currentZoom = tensionGraph.DataSource.HorizontalViewSize;
                mCurrentPageSizeFactor = PageSizeFactor * 0.9f;
            }
            LoadPage(tensionGraph.HorizontalScrolling);
            if (ControlViewPortion)
            {
                AdjustHorizontalView();
                AdjustVerticalView();
            }
        }
    }

    void LoadWithoutDownSampling(int start, int end)
    {
        for (int i = start; i < end; i++) // load the data
        {
            tensionGraph.DataSource.AddPointToCategory(Category, GreifbarUserPerformanceManager.instance.DataCollection[i].Time, GreifbarUserPerformanceManager.instance.DataCollection[i].Tension);
        }
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
                tensionGraph.DataSource.AddPointToCategory(Category, x, y);
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
        if (tensionGraph != null)
        {

            //Debug.Log("Loading page :" + pagePosition);
            tensionGraph.DataSource.StartBatch(); // call start batch 
            tensionGraph.DataSource.HorizontalViewOrigin = 0;
            int start, end;
            findPointsForPage(pagePosition, out start, out end); // get the page edges
            tensionGraph.DataSource.ClearCategory(Category); // clear the cateogry
            mStart = start;
            if (DownSampleToPoints <= 0)
                LoadWithoutDownSampling(start, end);
            else
                LoadWithDownSampling(start, end);
            tensionGraph.DataSource.EndBatch();
            tensionGraph.HorizontalScrolling = pagePosition;
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

