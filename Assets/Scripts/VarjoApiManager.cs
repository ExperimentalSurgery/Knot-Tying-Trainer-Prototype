using System;

using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using DFKI_Utilities;

public class VarjoApiManager : MonoBehaviour
{
    private class VarjoStream
    {
        [DllImport("VarjoStream")]
        unsafe public static extern void StartStreaming();

        [DllImport("VarjoStream")]
        unsafe public static extern void StopStreaming();

        [DllImport("VarjoStream")]
        unsafe public static extern byte* GetLeftImg();

        [DllImport("VarjoStream")]
        unsafe public static extern byte* GetRightImg();

        [DllImport("VarjoStream")]
        unsafe public static extern double* GetLeftExtrinsics();

        [DllImport("VarjoStream")]
        unsafe public static extern double* GetRightExtrinsics();

        [DllImport("VarjoStream")]
        unsafe public static extern double* GetLeftIntrinsics();

        [DllImport("VarjoStream")]
        unsafe public static extern double* GetRightIntrinsics();

        [DllImport("VarjoStream")]
        unsafe public static extern double* GetIntrinsicsUndistorted();

        [DllImport("VarjoStream")]
        unsafe public static extern void LockImages();

        [DllImport("VarjoStream")]
        unsafe public static extern void UnlockImages();

        [DllImport("VarjoStream")]
        unsafe public static extern void LockXtrinsics();

        [DllImport("VarjoStream")]
        unsafe public static extern void UnlockXtrinsics();

        [DllImport("VarjoStream")]
        unsafe public static extern void SetUndistort(bool activate);

        [DllImport("VarjoStream")]
        unsafe public static extern float* GetPointcloudVertices();

        [DllImport("VarjoStream")]
        unsafe public static extern float* GetPointcloudColors();

        [DllImport("VarjoStream")]
        unsafe public static extern float* GetPointcloudNormals();

        [DllImport("VarjoStream")]
        unsafe public static extern int* GetPointcloudIndices();
    }

    // controlling variables
    public bool capturing { get; set; } = false;
    private bool undistort = true;
    private bool streamEnabled = false;

    // current public settings
    public RawImage leftImg, rightImg;
    public Texture2D leftTexture, rightTexture;
    public CameraView cameraLeft, cameraRight;

    // hidden settings
    public int width = 1152;
    public int height = 1152;
    public int numChannels = 4;
    public int bytesPerChannel = 1; // i.e., 8 bits
    private int imgDim;
    private int leftImgs = 1;
    private int rightImgs = 1;
    private double[] extr_l, extr_r, intr_l, intr_r, intr_und;

    // Start is called before the first frame update
    void Start()
    {
        imgDim = width * height * numChannels * bytesPerChannel;
        
        rightTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        leftTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        cameraLeft = new CameraView("left", width, height);
        cameraRight = new CameraView("right", width, height);

        extr_l = new double[16];
        extr_r = new double[16];
        intr_l = new double[10];
        intr_r = new double[10];
        intr_und = new double[10];

        SetUndistort(undistort);
    }

    // Update is called once per frame
    void Update()
    {
        if (streamEnabled)
        {
            UpdateImages();
            UpdateXtrinsics();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Disable();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            // toggle undistort
            SetUndistort(!undistort);
        }

        if (capturing)
        {
            if (!streamEnabled)
                Enable();
        }

        //if (!capturing)
        //{
        //    if (streamEnabled)
        //        Disable();
        //}
    }

    public void SetUndistort(bool set)
    {
        undistort = set;
        unsafe
        {
            VarjoStream.SetUndistort(undistort);
        }

        if (undistort)
            Debug.Log("Enabling undistortion of streamed images");
        else
            Debug.LogWarning("Warning: Disabling undistortion of streamed images: The streamed images will require manual undistortion!");
    }

    public void Enable()
    {
        if (!streamEnabled)
        {
            VarjoStream.StartStreaming();
            streamEnabled = true;
        }
    }

    public void Disable()
    {
        if (streamEnabled)
        {
            VarjoStream.StopStreaming();
            streamEnabled = false;
        }
    }

    public bool IsEnabled()
    {
        return streamEnabled;
    }

    void UpdateImages()
    {
        unsafe
        {
            VarjoStream.LockImages();

            byte* imgptrL = VarjoStream.GetLeftImg();
            byte [] arrayL = new byte[imgDim];
            Marshal.Copy((IntPtr)imgptrL, arrayL, 0, imgDim);

            byte* imgptrR = VarjoStream.GetRightImg();
            byte [] arrayR = new byte[imgDim];
            Marshal.Copy((IntPtr)imgptrR, arrayR, 0, imgDim);

            VarjoStream.UnlockImages();

            if (arrayL != null && arrayR != null)
            {
                leftTexture.LoadRawTextureData(arrayL);
                leftTexture.Apply();
                leftImg.GetComponent<RawImage>().texture = leftTexture;

                rightTexture.LoadRawTextureData(arrayR);
                rightTexture.Apply();
                rightImg.GetComponent<RawImage>().texture = rightTexture;

                if (capturing)
                {
                    string filenameL = "varjo_left_" + leftImgs + ".bin";
                    string pathL = Application.dataPath + "/../Output/Images/" + filenameL;
                    File.WriteAllBytes(pathL, arrayL);
                    leftImgs++;

                    string filenameR = "varjo_right_" + rightImgs + ".bin";
                    string pathR = Application.dataPath + "/../Output/Images/" + filenameR;
                    File.WriteAllBytes(pathR, arrayR);
                    rightImgs++;

                }
            }
        }
    }

    void UpdateXtrinsics()
    {
        unsafe
        {
            VarjoStream.LockXtrinsics();

            double* extr_l_ptr = VarjoStream.GetLeftExtrinsics();
            double* intr_l_ptr = VarjoStream.GetLeftIntrinsics();
            double* extr_r_ptr = VarjoStream.GetRightExtrinsics();
            double* intr_r_ptr = VarjoStream.GetRightIntrinsics();
            double* intr_und_ptr = VarjoStream.GetIntrinsicsUndistorted();

            Marshal.Copy((IntPtr)extr_l_ptr, extr_l, 0, 16);
            Marshal.Copy((IntPtr)extr_r_ptr, extr_r, 0, 16);
            Marshal.Copy((IntPtr)intr_l_ptr, intr_l, 0, 10);
            Marshal.Copy((IntPtr)intr_r_ptr, intr_r, 0, 10);
            Marshal.Copy((IntPtr)intr_und_ptr, intr_und, 0, 10);

            VarjoStream.UnlockXtrinsics();
        }

        cameraLeft.SetXTrinsics(intr_und, extr_l);
        cameraRight.SetXTrinsics(intr_und, extr_r);
    }

    public void SaveCameraXTrinsics()
    {
        string header = "# Double precision 4x4 matrix. The matrix usage convention is that they are stored in column-major order and transforms are stacked before column-vector points when multiplying. That is, a pure translation matrix will have the position offset in elements 12..14. Unless otherwise specified, the coordinate system is right-handed: X goes right, Y goes up and negative Z goes forward.";
        SaveArrayToFile("RightCameraExtrinsics.txt", extr_r, 16, header);
        SaveArrayToFile("LeftCameraExtrinsics.txt", extr_l, 16, header);

        header = "# principalPointX principalPointY focalLengthX focalLengthY distortionCoefficient_1 distortionCoefficient_2 distortionCoefficient_3 distortionCoefficient_4 distortionCoefficient_5 distortionCoefficient_6";
        SaveArrayToFile("RightCameraIntrinsics.txt", intr_r, 10, header);
        SaveArrayToFile("LeftCameraIntrinsics.txt", intr_l, 10, header);
        SaveArrayToFile("CameraIntrinsicsUndistorted.txt", intr_und, 10, header);
    }

    void SavePointcloud()
    {
        unsafe
        {
            float* vertices = VarjoStream.GetPointcloudVertices();
            //float* colors = GetPointcloudColors();
            //float* normals = GetPointcloudNormals();
            //int* indices = GetPointcloudIndices();

            if (vertices != null)// && colors != null && normals != null && indices != null)
            {
                //int sizei = 1 + (int)indices[0] * 3; // total number values into the array
                //int i_size = (sizei - 1) / 3;
                int size = 1 + (int) vertices[0] * 3; // total number values into the array
                int v_size = (size - 1) / 3; // number of vertices/normals/colors

                float[] v = new float[size];
                Marshal.Copy((IntPtr)vertices, v, 0, size);

                //int[] c = new int[size];
                //Marshal.Copy((IntPtr)colors, c, 0, size);

                //int[] idx = new int[sizei];
                //Marshal.Copy((IntPtr)indices, idx, 0, sizei);

                string path = Application.dataPath + "/../Output/" + "Pointcloud.ply";

                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine("ply");
                    sw.WriteLine("format ascii 1.0");
                    sw.WriteLine("element vertex " + v_size);
                    sw.WriteLine("property float32 x");
                    sw.WriteLine("property float32 y");
                    sw.WriteLine("property float32 z");
                    //sw.WriteLine("property uchar red");
                    //sw.WriteLine("property uchar green");
                    //sw.WriteLine("property uchar blue");
                    //sw.WriteLine("element face " + size);
                    //sw.WriteLine("property list uint8 int32 vertex_index");
                    sw.WriteLine("end_header");

                    for (int i = 0; i < v_size; i++)
                    {
                        sw.WriteLine(
                            v[1 + i * 3].ToString() + " " + v[1 + i * 3 + 1].ToString() + " " + v[1 + i * 3 + 2].ToString()); //+ " " +
                            //c[1 + i * 3].ToString() + " " + c[1 + i * 3 + 1].ToString() + " " + c[1 + i * 3 + 2].ToString());
                    }

                    //for (int i = 0; i + 2 < v_size; i += 3)
                    //{
                    //    sw.WriteLine("3 " + i.ToString() + " " + (i + 1).ToString() + " " + (i + 2).ToString());
                    //}
                }
                Debug.Log("Data saved successfully");
            }
        }
    }

    void SaveArrayToFile(string filename, double[] array, int arrayLen, string header)
    {
        string path = Application.dataPath + "/../Output/" + filename;

        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.WriteLine(header);

            for(int i=0; i < arrayLen; i++)
            {
                sw.WriteLine(array[i].ToString());
            }
        }
        Debug.Log("Data saved successfully");
    }

    void OnApplicationQuit()
    {
        Disable();
    }
}
