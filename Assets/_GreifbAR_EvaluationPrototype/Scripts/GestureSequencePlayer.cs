using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NMY;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace DFKI.NMY
{

    [Serializable]
    public class HandGestureParams
    {
        public bool isMatching;
        public bool leftHand = true; // 1=left, 0=right
        public int sequenceIndex;
    }
    
    
public class GestureSequencePlayer : SingletonStartupBehaviour<GestureSequencePlayer>
{
    
    [Tooltip("Threshold for pose matching. Good default = 25")]
    public float poseMatchingThreshold = 25;
    
    public BVHRecorder leftRecorder;
    public BVHRecorder rightRecorder;

    
    public UnityEvent<HandGestureParams> SequenceFinishedEvent = new UnityEvent<HandGestureParams>(); 
    public bool isPlayingLeft = false;
    public bool isPlayingRight = false;
    public float normalizedProgressLeft;
    public float normalizedProgressRight;
    
    [Tooltip("GameObject of left expert hand")]
    public GameObject leftExpertHand;
    [Tooltip("GameObject of right expert hand")]
    public GameObject rightExpertHand;

    public float sequenceDuration = 5f;
    public int currentSequenceLeft = 0;
    public int currentSequenceRight = 0;
    // test
    [Tooltip("The directory where BVH data will be stored or read from")]
    [SerializeField] private string bvhRelativeDirectory = "/../BVHdata/";
    
    // Consts
    private const string BvhSuffix = ".bvh";
    private string BvhDirectory => Application.dataPath + bvhRelativeDirectory;

    // Gesture Module Class Instances    
    private readonly DFKI.GestureModule _leftGestureModule = new();
    private readonly DFKI.GestureModule _rightGestureModule = new();

    private bool playAllSequences = false;
    private bool loopSequence = false;
    private int currentFrameLeft = 0;
    private int currentFrameRight = 0;

    private float loopStartTimeLeft;
    private float loopStartTimeRight;
    private float loopEndTimeLeft => loopStartTimeLeft + sequenceDuration;
    private float loopEndTimeRight => loopStartTimeRight + sequenceDuration;


    private bool[] left_hand_compare_mask;
    private bool[] right_hand_compare_mask;

    protected void ProcessFramePlayback()
    {
        if (isPlayingLeft)
        {
            ApplyBVHFrame(_leftGestureModule.GetFrame(currentFrameLeft), leftExpertHand);

            if (currentFrameLeft >= _leftGestureModule.GetSequenceEndFrameIndex(currentSequenceLeft)) {
                if (playAllSequences)
                {
                    currentSequenceLeft++;
                }

                loopStartTimeLeft = Time.time;
                currentFrameLeft = _leftGestureModule.GetSequenceStartFrameIndex(currentSequenceLeft);
            }
            if (currentSequenceLeft >= _leftGestureModule.GetNumberOfSequences()-1) {
                isPlayingLeft = false;
            }
          
            normalizedProgressLeft = (Time.time - loopStartTimeLeft) / (loopEndTimeLeft - loopStartTimeLeft);
            currentFrameLeft = _leftGestureModule.GetSequenceStartFrameIndex(currentSequenceLeft) + (int)(_leftGestureModule.GetSequenceLength(currentSequenceLeft) * normalizedProgressLeft);
            
        }

        if (isPlayingRight)
        {
            ApplyBVHFrame(_rightGestureModule.GetFrame(currentFrameRight), rightExpertHand);
            if (currentFrameRight >= _rightGestureModule.GetSequenceEndFrameIndex(currentSequenceRight)) {
                
                if(playAllSequences){ currentSequenceRight++;}
                loopStartTimeRight = Time.time;
                currentFrameRight = _rightGestureModule.GetSequenceStartFrameIndex(currentSequenceRight);
            }
            if (currentSequenceRight >= _rightGestureModule.GetNumberOfSequences()-1) {
                isPlayingRight = false;
            }
            normalizedProgressRight = (Time.time - loopStartTimeRight) / (loopEndTimeRight - loopStartTimeRight);
            currentFrameRight = _rightGestureModule.GetSequenceStartFrameIndex(currentSequenceRight) + (int)(_rightGestureModule.GetSequenceLength(currentSequenceRight) * normalizedProgressRight);
        }
    }
    
    
    private void Update()
    {
       ProcessFramePlayback();
       AnalyzePoseMatch();

       if (Application.isEditor)
       {
           // Simulate left finished event
           if (Input.GetKeyDown(KeyCode.Alpha1))
           {
               HandGestureParams gestureParams = new HandGestureParams();
               gestureParams.isMatching = true;
               gestureParams.leftHand = true;
               gestureParams.sequenceIndex = currentSequenceLeft;
               SequenceFinishedEvent.Invoke(gestureParams);
           }
           
           // Simulate right finished event
           if (Input.GetKeyDown(KeyCode.Alpha2))
           {
               HandGestureParams gestureParams = new HandGestureParams();
               gestureParams.isMatching = true;
               gestureParams.leftHand = false;
               gestureParams.sequenceIndex = currentSequenceRight;
               SequenceFinishedEvent.Invoke(gestureParams);
           }
       }
    }

    protected void AnalyzePoseMatch() {
       
        // get current life user frames from headset
        float[] user_frame_data_left = GetFrame(leftRecorder);
        float[] user_frame_data_right = GetFrame(rightRecorder);
        
        // compare user frames with end poses
        if (_leftGestureModule.SimilarPose(_leftGestureModule.GetSequenceEndFrameIndex(currentSequenceLeft), user_frame_data_left,
                poseMatchingThreshold, left_hand_compare_mask)) {

            HandGestureParams gestureParams = new HandGestureParams();
            gestureParams.isMatching = true;
            gestureParams.leftHand = true;
            gestureParams.sequenceIndex = currentSequenceLeft;
            SequenceFinishedEvent.Invoke(gestureParams);
                
        }
        // compare user frames with end poses
        if (_rightGestureModule.SimilarPose(_rightGestureModule.GetSequenceEndFrameIndex(currentSequenceRight), user_frame_data_right,
                poseMatchingThreshold, right_hand_compare_mask)) {
        
            HandGestureParams gestureParams = new HandGestureParams();
            gestureParams.isMatching = true;
            gestureParams.leftHand = false;
            gestureParams.sequenceIndex = currentSequenceRight;
            SequenceFinishedEvent.Invoke(gestureParams);
            
        }

    }


    public void ChangeDuration(float newDuration)
    {
        sequenceDuration = newDuration;
        loopStartTimeLeft = Time.time - sequenceDuration * normalizedProgressLeft;
        loopStartTimeRight = Time.time - sequenceDuration * normalizedProgressRight;
    }

    protected override void StartupEnter()
    {
        base.StartupEnter();
        InitSequence("recording_left","recording_right");
        Play();
    }

    public void Stop()
    {
        isPlayingLeft = false;
        isPlayingRight = false;
        playAllSequences = false;
    }

    public void Play(int singleSequence=-1)
    {
        if (singleSequence > 0) {
            playAllSequences = false;
        }
        
        // setup
        currentSequenceLeft = singleSequence >= 0 ? singleSequence : 0;
        currentSequenceRight = singleSequence >= 0 ? singleSequence : 0;
        currentFrameLeft = _leftGestureModule.GetSequenceStartFrameIndex(currentSequenceLeft);
        currentFrameRight = _rightGestureModule.GetSequenceStartFrameIndex(currentSequenceRight);
        loopStartTimeLeft = Time.time;
        loopStartTimeRight = Time.time;
        isPlayingLeft = true;
        isPlayingRight = true;
    }

    public void InitSequence(string leftHandBvhFilename, string rightHandBvhFilename) {

        // load and pre-process the expert BVH files
        _leftGestureModule.BVHPreprocessing(BvhDirectory + leftHandBvhFilename + BvhSuffix);
        _rightGestureModule.BVHPreprocessing(BvhDirectory + rightHandBvhFilename + BvhSuffix);
        
        
        // generate compare masks - i.e. ignore first 6 parameters on both hands
        left_hand_compare_mask = new bool[_leftGestureModule.GetParameterCount()];
        for (int i = 0; i < 6; i++) {
            left_hand_compare_mask[i] = false;
        }
        for (int i = 6; i < left_hand_compare_mask.Length; i++) {
            left_hand_compare_mask[i] = true;
        }
        right_hand_compare_mask = new bool[_rightGestureModule.GetParameterCount()];
        for (int i = 0; i < 6; i++) {
            right_hand_compare_mask[i] = false;
        }
        for (int i = 6; i < right_hand_compare_mask.Length; i++) {
            right_hand_compare_mask[i] = true;
        }
        
        
        
        // setup life user recorders
        if (leftRecorder == null) {
            Debug.Log("Left recorder is NULL!");
        }
        leftRecorder.setVoilatileCapturing(true);
        leftRecorder.setCapturing(true);

        if (rightRecorder == null) {
            Debug.Log("Right recorder is NULL!");
        }
        rightRecorder.setVoilatileCapturing(true);
        rightRecorder.setCapturing(true);
        
    }
    
    private float[] GetFrame(BVHRecorder recorder) {
        string frame = recorder.getLastFrame();
        if (frame == null)
            return null;
        //Debug.Log("Capture Frame Length: " + frame);
        return ParseFloats(frame);
    }
    
    
    private float[] ParseFloats(string input) {
        // Cleanup consecutive spaces
        string line = Regex.Replace(input, @"\s{1,}", " ");
        line = line.Trim();

        string[] tokens = line.Split(' ');

        List<float> floats = new List<float>();

        foreach (string token in tokens) {
            if (float.TryParse(token, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float value)) {
                floats.Add(value);
            }
        }

        return floats.ToArray();
    }
    
        void ApplyBVHFrame(float[] bvhFrame, GameObject model) {

        // Adaption of Coordinates
        int i = 4, j = 3, k = 5;
        int sign1 = 1, sign2 = -1, sign3 = 1;


        Transform elbow = model.transform.Find("Elbow");

        // L_Wrist
        i += 3; j += 3; k += 3;
        Transform L_Wrist = elbow.transform.Find("L_Wrist");
        //L_Wrist.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);


        // L_index_Proximal
        i += 3; j += 3; k += 3;
        Transform L_index_Proximal = L_Wrist.transform.Find("L_index_Proximal");
        L_index_Proximal.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);

        // L_index_b
        i += 3; j += 3; k += 3;
        Transform L_index_b = L_index_Proximal.transform.Find("L_index_b");
        L_index_b.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);

        // L_index_c
        i += 3; j += 3; k += 3;
        Transform L_index_c = L_index_b.transform.Find("L_index_c");
        L_index_c.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);

        // L_middle_Proximal
        i += 3; j += 3; k += 3;
        Transform L_middle_Proximal = L_Wrist.transform.Find("L_middle_Proximal");
        L_middle_Proximal.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);

        // L_middle_b
        i += 3; j += 3; k += 3;
        Transform L_middle_b = L_middle_Proximal.transform.Find("L_middle_b");
        L_middle_b.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);


        // L_middle_c
        i += 3; j += 3; k += 3;
        Transform L_middle_c = L_middle_b.transform.Find("L_middle_c");
        L_middle_c.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);

        // L_pinky_Proximal
        i += 3; j += 3; k += 3;
        Transform L_pinky_Proximal = L_Wrist.transform.Find("L_pinky_Proximal");
        L_pinky_Proximal.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);

        // L_pinky_b
        i += 3; j += 3; k += 3;
        Transform L_pinky_b = L_pinky_Proximal.transform.Find("L_pinky_b");
        L_pinky_b.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);


        // L_pinky_c
        i += 3; j += 3; k += 3;
        Transform L_pinky_c = L_pinky_b.transform.Find("L_pinky_c");
        L_pinky_c.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);

        // L_ring_Proximal
        i += 3; j += 3; k += 3;
        Transform L_ring_Proximal = L_Wrist.transform.Find("L_ring_Proximal");
        L_ring_Proximal.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);

        // L_ring_b
        i += 3; j += 3; k += 3;
        Transform L_ring_b = L_ring_Proximal.transform.Find("L_ring_b");
        L_ring_b.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);


        // L_ring_c
        i += 3; j += 3; k += 3;
        Transform L_ring_c = L_ring_b.transform.Find("L_ring_c");
        L_ring_c.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);

        // L_thumb_Proximal
        i += 3; j += 3; k += 3;
        Transform L_thumb_Proximal = L_Wrist.transform.Find("L_thumb_Proximal");
        L_thumb_Proximal.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);

        // L_thumb_a
        i += 3; j += 3; k += 3;
        Transform L_thumb_a = L_thumb_Proximal.transform.Find("L_thumb_a");
        L_thumb_a.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);


        // L_thumb_b
        i += 3; j += 3; k += 3;
        Transform L_thumb_b = L_thumb_a.transform.Find("L_thumb_b");
        L_thumb_b.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);

    }


}

}
