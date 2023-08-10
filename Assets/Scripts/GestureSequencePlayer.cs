using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NMY;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace DFKI.NMY
{

    [Serializable]
    public class HandGestureParams
    {
        public bool isMatching;
        public Hand side;
        public int sequenceIndex;
    }
    
    public enum Hand{Left=0,Right=1}
    
public class GestureSequencePlayer : SingletonStartupBehaviour<GestureSequencePlayer>
{

    [Header("Config")] 
    [SerializeField] private bool analyzePoseMatching = true;
    [SerializeField] private bool loopAllSequences = false;
    [SerializeField] private bool playAllSequences = false;
    [SerializeField] private bool loopSingleSequencePlayback = false;
    [SerializeField] private bool autoStart = false;
    [Tooltip("Threshold for pose matching. Good default = 25")]
    [SerializeField] private float poseMatchingThreshold = 25;
    [Tooltip("The directory where BVH data will be stored or read from")]
    [SerializeField] private string bvhRelativeDirectory = "/../BVHdata/";

    [SerializeField] private string leftHandBvhFile = "recording_left";
    [SerializeField] private string rightHandBvhFile = "recording_right";
    [SerializeField] private float sequenceDuration = 5f;

    [Header("References")]
    [SerializeField] private BVHRecorder leftRecorder;
    [SerializeField] private BVHRecorder rightRecorder;
    [Tooltip("GameObject of left expert hand")] public GameObject leftExpertHand;
    [Tooltip("GameObject of right expert hand")] public GameObject rightExpertHand;

    public Vector3 rightHandPosMod;
    public Vector3 rightHandRotMod;
    public Vector3 originOffset;

    [SerializeField] private bool useReducedSpeed = false;
    
    // Consts
    private const string BvhSuffix = ".bvh";
   
    private float loopEndTimeLeft => loopStartTimeLeft + sequenceDuration;
    private float loopEndTimeRight => loopStartTimeRight + sequenceDuration;

    private float currentSpeedMultiplier = 1f;
    private float initialSequenceDuration =5;
    
    // Properties
    public bool AnalyzePoseMatching
    {
        get => analyzePoseMatching;
        set => analyzePoseMatching = value;
    }

    public bool LoopSingleSequencePlayback
    {
        get => loopSingleSequencePlayback;
        set => loopSingleSequencePlayback = value;
    }

    public bool LoopAllSequences
    {
        get => loopAllSequences;
        set => loopAllSequences = value;
    }

    public string BvhRelativeDirectory
    {
        get => bvhRelativeDirectory;
        set => bvhRelativeDirectory = value;
    }

    public float SequenceDuration
    {
        get => sequenceDuration;
        set => sequenceDuration = value;
    }

    public float PoseMatchingThreshold
    {
        get => poseMatchingThreshold;
        set => poseMatchingThreshold = value;
    }

    public string LeftHandBvhFile
    {
        get => leftHandBvhFile;
        set => leftHandBvhFile = value;
    }

    public string RightHandBvhFile
    {
        get => rightHandBvhFile;
        set => rightHandBvhFile = value;
    }

    public bool PlayAllSequences
    {
        get => playAllSequences;
        set => playAllSequences = value;
    }

    public string BvhDirectory => Application.dataPath + bvhRelativeDirectory;


    
    [Header("Runtime Variables")]
    // Runtime vars with public access
    public float normalizedProgressLeft;
    public float normalizedProgressRight;
    public float normalizedProgressTotalLeft;
    public float normalizedProgressTotalRight;
    public int currentFrameLeft = 0;
    public int currentFrameRight = 0;
    public int currentSequenceLeft = 0;
    public int currentSequenceRight = 0;
    public bool isPlayingLeft = false;
    public bool isPlayingRight = false;
    public bool isPausedLeft = false;
    public bool isPausedRight = false;
    
    // Private runtime vars
    private bool[] left_hand_compare_mask;
    private bool[] right_hand_compare_mask;
    private float loopStartTimeLeft;
    private float loopStartTimeRight;
    private List<int> playedFramesLeft;
    private List<int> playedFramesRight;

    // Gesture Module Class Instances    
    private readonly DFKI.GestureModule _leftGestureModule = new();
    private readonly DFKI.GestureModule _rightGestureModule = new();
    
    [Header("Events")]
    // Events
    public UnityEvent<HandGestureParams> SequenceFinishedEvent = new UnityEvent<HandGestureParams>();
    public UnityEvent<HandGestureParams> AllSequencesPlayedEvent = new UnityEvent<HandGestureParams>();

    protected override void StartupEnter()
    {
        base.StartupEnter();
        if (autoStart)
        {
            InitSequence();
            Play(11,11);
        }
    }

    public void ToggleSpeed(bool useDefault=false)
    {
        useReducedSpeed = !useDefault && !useReducedSpeed;
        currentSpeedMultiplier = useReducedSpeed ? 2.0f : 1.0f;
        ChangeDuration(initialSequenceDuration);
    }
    
    public void ProcessFrame(
        GestureModule gestureModule, 
        ref int currentFrame,
        ref int currentSequence,
        GameObject expertHand,
        ref float startTime,
        float endTime,
        ref bool isPlaying,
        ref List<int> playedFrames,
        ref float normalizedProgress,
        ref float normalizedProgressTotal,
        Hand side) {

        if (!isPlaying) return;
        
        ApplyBVHFrame(gestureModule.GetFrame(currentFrame), expertHand);

            
            bool reachedEndFrame = currentFrame >= gestureModule.GetSequenceEndFrameIndex(currentSequence);
            bool reachedLastSequence = (currentSequence >= gestureModule.GetNumberOfSequences() - 1);
            if (reachedEndFrame) {

                if (playAllSequences) {
                    startTime = Time.time;
                    currentSequence = reachedLastSequence? 0 : currentSequence+1;
                    currentFrame = gestureModule.GetSequenceStartFrameIndex(currentSequence);
                }
                else if (!playAllSequences && loopSingleSequencePlayback)
                {
                    startTime = Time.time;
                    currentFrame = gestureModule.GetSequenceStartFrameIndex(currentSequence);
                }
                else
                {
                    Stop();
                }
            }
        
            // BVH File All Sequences Finish check
            if (reachedEndFrame && reachedLastSequence)
            {
                if (loopAllSequences) {
                    
                    // reset runtime params
                    isPlaying = true;
                    playedFrames.Clear();
                    currentSequence = 0;
                    
                    // event params
                    HandGestureParams gestureParams = new HandGestureParams();
                    gestureParams.isMatching = false;
                    gestureParams.side = side;
                    gestureParams.sequenceIndex = gestureModule.GetNumberOfSequences();
                    AllSequencesPlayedEvent.Invoke(gestureParams);
                }
                else {
                    Stop();
                }
            }

            if (isPlaying)
            {
                normalizedProgress = (Time.time - startTime) / (endTime - startTime);
                if (playAllSequences)
                {
                    normalizedProgressTotal = (float)playedFrames.Count / GetTotalFramesForAllSequences(gestureModule);
                }
                else
                {
                    normalizedProgressTotal = normalizedProgress;
                }

                currentFrame = gestureModule.GetSequenceStartFrameIndex(currentSequence) +
                               (int)(gestureModule.GetSequenceLength(currentSequence) *
                                     normalizedProgress);
                if (!playedFrames.Contains(currentFrame)) {
                    playedFrames.Add(currentFrame);
                }
                
                //Debug.Log("seq:"+currentSequence+" sf:"+gestureModule.GetSequenceStartFrameIndex(currentSequence)+ " ef:"+gestureModule.GetSequenceEndFrameIndex(currentSequence)+" p:"+playedFrames.Count+" f:"+currentFrame);

            }
            
           
        }
    
    private void Update()
    {
        ProcessFrame(_leftGestureModule, ref currentFrameLeft, ref currentSequenceLeft, leftExpertHand, ref loopStartTimeLeft, loopEndTimeLeft, ref isPlayingLeft, ref playedFramesLeft, ref normalizedProgressLeft, ref normalizedProgressTotalLeft, Hand.Left);
        ProcessFrame(_rightGestureModule, ref currentFrameRight, ref currentSequenceRight, rightExpertHand, ref loopStartTimeRight, loopEndTimeRight, ref isPlayingRight, ref playedFramesRight, ref normalizedProgressRight, ref normalizedProgressTotalRight, Hand.Right);


        if (Application.isEditor)
        {

            if (Input.GetKeyDown(KeyCode.P))
            {
                Pause();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                Resume();
            }

        }

        if (AnalyzePoseMatching)
        {
            AnalyzePoseMatch();
            
            if (Application.isEditor)
            {
                
                // Simulate left finished event
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    HandGestureParams gestureParams = new HandGestureParams();
                    gestureParams.isMatching = true;
                    gestureParams.side = Hand.Left;
                    gestureParams.sequenceIndex = currentSequenceLeft;
                    SequenceFinishedEvent.Invoke(gestureParams);
                }

                // Simulate right finished event
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    HandGestureParams gestureParams = new HandGestureParams();
                    gestureParams.isMatching = true;
                    gestureParams.side = Hand.Right;
                    gestureParams.sequenceIndex = currentSequenceRight;
                    SequenceFinishedEvent.Invoke(gestureParams);
                }
            }
        }
    }


    public int GetTotalFramesForAllSequences(GestureModule gm)
    {
        int c = 0;
        for (int i = 0; i < gm.GetNumberOfSequences(); i++) {
            c += gm.GetSequenceLength(i);
        }
        return c;
    }
    
    
    protected void AnalyzePoseMatch() {
       
        
        if (isPlayingLeft)
        {
            
            // get current life user frames from headset
            float[] user_frame_data_left = GetFrame(leftRecorder);
            // compare user frames with end poses
            if (_leftGestureModule.SimilarPose(_leftGestureModule.GetSequenceEndFrameIndex(currentSequenceLeft), user_frame_data_left, poseMatchingThreshold, left_hand_compare_mask)) {
                HandGestureParams gestureParams = new HandGestureParams();
                gestureParams.isMatching = true;
                gestureParams.side = Hand.Left;
                gestureParams.sequenceIndex = currentSequenceLeft;
                SequenceFinishedEvent.Invoke(gestureParams);

            }
        }

        if (isPlayingRight)
        {
            // get current life user frames from headset
            float[] user_frame_data_right = GetFrame(rightRecorder);
            // compare user frames with end poses
            if (_rightGestureModule.SimilarPose(_rightGestureModule.GetSequenceEndFrameIndex(currentSequenceRight), user_frame_data_right, poseMatchingThreshold, right_hand_compare_mask)) {
                HandGestureParams gestureParams = new HandGestureParams();
                gestureParams.isMatching = true;
                gestureParams.side = Hand.Right;
                gestureParams.sequenceIndex = currentSequenceRight;
                SequenceFinishedEvent.Invoke(gestureParams);
                HandVisualizer.instance.SetSuccessOutline(false, true);
            }
        }

    }


    public void ChangeDuration(float newDuration)
    {
        sequenceDuration = newDuration * currentSpeedMultiplier;
        loopStartTimeLeft = Time.time - sequenceDuration * normalizedProgressLeft;
        loopStartTimeRight = Time.time - sequenceDuration * normalizedProgressRight;
    }

    public void Stop()
    {
        isPlayingLeft = false;
        isPlayingRight = false;
        isPausedLeft = false;
        isPausedRight = false;
        playAllSequences = false;
        normalizedProgressLeft = 0.0f;
        normalizedProgressRight = 0.0f;
        normalizedProgressTotalLeft = 0.0f;
        normalizedProgressTotalRight = 0.0f;
    }


    public void Play(int singleSequenceLeft=-1,int singleSequenceRight=-1)
    {
        
        if (singleSequenceLeft > 0) {
            playAllSequences = false;
        }

        
        // backup vars
        initialSequenceDuration = SequenceDuration;
        
        // setup
        currentSequenceLeft = singleSequenceLeft >= 0 ? singleSequenceLeft : 0;
        currentSequenceRight = singleSequenceRight >= 0 ? singleSequenceRight : 0;
        currentFrameLeft = _leftGestureModule.GetSequenceStartFrameIndex(currentSequenceLeft);
        currentFrameRight = _rightGestureModule.GetSequenceStartFrameIndex(currentSequenceRight);
        loopStartTimeLeft = Time.time;
        loopStartTimeRight = Time.time;
        isPlayingLeft = true;
        isPlayingRight = true;
        playedFramesLeft = new List<int>();
        playedFramesRight = new List<int>();

    }

    public void Pause()
    {
        isPausedLeft = true;
        isPausedRight = true;
        isPlayingLeft = false;
        isPlayingRight = false;
    }

    public void Resume()
    {
        isPausedLeft = false;
        isPausedRight = false;
        isPlayingLeft = true;
        isPlayingRight = true;
    }

    public void InitSequence() {

        
        // load and pre-process the expert BVH files
        _leftGestureModule.BVHPreprocessing(BvhDirectory + leftHandBvhFile + BvhSuffix);
        _rightGestureModule.BVHPreprocessing(BvhDirectory + rightHandBvhFile + BvhSuffix);
        
        
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
        if (elbow.parent.name == "Right")
        {
            elbow.localPosition = new Vector3(rightHandPosMod.x *( bvhFrame[0] + originOffset.x), rightHandPosMod.y *( bvhFrame[1] + originOffset.y), rightHandPosMod.z * (bvhFrame[2] + originOffset.z));
            elbow.localEulerAngles = new Vector3(rightHandRotMod.x * bvhFrame[i], rightHandRotMod.y * bvhFrame[j], rightHandRotMod.z * bvhFrame[k]);
        }
        else
        {
            elbow.localPosition = new Vector3(-1 * (bvhFrame[0] + originOffset.x), sign2 * (bvhFrame[1] + originOffset.y), sign3 * (bvhFrame[2] + originOffset.z));
            elbow.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);
        }


        // L_Wrist
        i += 3; j += 3; k += 3;
        Transform L_Wrist = elbow.transform.Find("L_Wrist");
        L_Wrist.localEulerAngles = new Vector3(sign1 * bvhFrame[i], sign2 * bvhFrame[j], sign3 * bvhFrame[k]);


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
