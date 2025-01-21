using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BVHManager : MonoBehaviour
{
    
    // Gesture Module Class Instances    
    private readonly DFKI.GestureModule _leftGestureModule = new();
    private readonly DFKI.GestureModule _rightGestureModule = new();

    [Header("Auto Record")] 
    [SerializeField] private bool autoRecord=false;
    [SerializeField] private AudioSource startrecordSFX;
    [SerializeField] private float autoRecordDelay = 5;
    
    public BVHRecorder leftRecorder;
    public BVHRecorder rightRecorder;
    public int recordingTimeLimit = 0;
    public string filename;
    //public VarjoApiManager varjoApiManager;

    private bool capturing = false;

    private Color redColor = new Color(1f, 0.1f, 0.1f, 1f);
    private Color whiteColor = new Color(1f, 1f, 1f, 1f);

    private float timeElapsed = 0f;
    private bool isCounting = false;
    private float remainingDelay;
    private TextMeshProUGUI recordbuttonText;
    
    // Start is called before the first frame update
    void Start()
    {
        if (autoRecord) {
            remainingDelay = autoRecordDelay;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (autoRecord)
        {
            remainingDelay -= Time.deltaTime;

            if (remainingDelay <= 0)
            {
                RecordAnimation(null);
                autoRecord = false;
            }
        }

        RunTimer();
    }

    private string currentFileLeft;
    private string currentFileRight;

    public void RecordAnimation(Button button)
    {

        if (startrecordSFX)
        {
            startrecordSFX.Play();
        }
        if (button)
        {
            recordbuttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        }

        leftRecorder.voilatile = false;
        rightRecorder.voilatile = false;

        leftRecorder.filename = filename + "_left";
        rightRecorder.filename = filename + "_right";

        leftRecorder.directory = Application.streamingAssetsPath + "/BVHdata";
        rightRecorder.directory = Application.streamingAssetsPath + "/BVHdata";

        currentFileLeft = leftRecorder.directory + "/" + leftRecorder.filename+".bvh";
        currentFileRight = rightRecorder.directory + "/" + rightRecorder.filename+".bvh";
        
        ToggleCapturing();

        if (!capturing)
        {
            SaveRecording();
            if (button)
            {
                recordbuttonText.SetText("Record");
                recordbuttonText.color = whiteColor;
            }
        }
        else
        {
            isCounting = true;
            if (button)
            {
                recordbuttonText.SetText("Stop");
                recordbuttonText.color = redColor;
            }
        }
    }

    private void ToggleCapturing()
    {
        capturing = !capturing;
        if (capturing) {
            leftRecorder.clearCapture();
            rightRecorder.clearCapture();
        }
        //varjoApiManager.capturing = capturing;
        leftRecorder.capturing = capturing;
        rightRecorder.capturing = capturing;
     }

    private void SaveRecording()
    {
        leftRecorder.saveBVH();
        rightRecorder.saveBVH();
        //varjoApiManager.SaveCameraXTrinsics();
    }


    private void RunTimer()
    {
        if (isCounting)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= recordingTimeLimit && recordingTimeLimit != 0) {
                StopTimer();
            }
        }
    }
    private void StopTimer() {

        if (startrecordSFX) {
            startrecordSFX.Play();
        }
        
        isCounting = false;
        timeElapsed = 0f;
        ToggleCapturing();
        SaveRecording();
        if (recordbuttonText)
        {
            recordbuttonText.SetText("Record");
            recordbuttonText.color = whiteColor;
        }
        
        _leftGestureModule.BVHPreprocessing(currentFileLeft);
        _rightGestureModule.BVHPreprocessing(currentFileRight);

        Debug.Log("Recording stopped!");
    }
}
