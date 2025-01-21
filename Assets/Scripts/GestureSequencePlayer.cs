using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Leap.Unity;
using NMY;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DFKI.NMY
{

    [Serializable]
    public class HandGestureParams
    {
        public bool isMatching;
        public Hand side;
        public bool passedMinDuration;
        public int sequenceIndex;
    }
    
    public enum Hand{Left=0,Right=1}
    
    public class GestureSequencePlayer : SingletonStartupBehaviour<GestureSequencePlayer>
    {

        [Header("DistanceCheck")] 
        public bool checkDistance = true;
        public Transform knotbankCenter;
        public Transform leftHandAnchor;
        public Transform rightHandAnchor;
        public float maxDistance = 0.4f;
        
        [Header("Autostart")]
        [SerializeField] private bool autoStart = false;
        [SerializeField] private int autoStartSequenceLeft = 0;
        [SerializeField] private int autoStartSequenceRight = 0;
    
        [Header("Config")] 
        [SerializeField] private bool analyzePoseMatching = true;
        [SerializeField] private bool loopAllSequences = false;
        [SerializeField] private bool playAllSequences = false;
        [SerializeField] private bool loopSingleSequencePlayback = false;
        [SerializeField] private bool useReducedSpeed = false;

        [FormerlySerializedAs("poseMatchingThreshold")]
        [Tooltip("Threshold for pose matching. Good default = 25")]
        [SerializeField] [Range(0,100)] private float poseMatchingThresholdRight = 25;
        [SerializeField] [Range(0,100)] private float poseMatchingThresholdLeft = 25;
        [SerializeField] private float poseMatchingMinDuration = 0.75f;
        [SerializeField] [Range(0,10)] private float sequenceDuration = 5f;

        [Header("Hand Offsets")]
        public Vector3 rightHandPosMod = new Vector3(x:-1,y:-1, z:1);
        public Vector3 rightHandRotMod = new Vector3(x: 1, y: 1, z: -1);
        public Vector3 originOffset;

    
        [Header("File References")]
        [FormerlySerializedAs("bvhRelativeDirectory")]
        [Tooltip("The directory where BVH data will be stored or read from")]
        [SerializeField] private string streamingAssetsSubDirectory = "BVHdata";

        [SerializeField] private string leftHandBvhFile = "recording_left";
        [SerializeField] private string rightHandBvhFile = "recording_right";
   
        [Header("References")]
        [SerializeField] private BVHRecorder leftRecorder;
        [SerializeField] private BVHRecorder rightRecorder;
        [Tooltip("GameObject of left expert hand")] public GameObject leftExpertHand;
        [Tooltip("GameObject of right expert hand")] public GameObject rightExpertHand;
        [SerializeField] public HandModelBase leftHand;
        [SerializeField] public HandModelBase rightHand;
    
        [SerializeField] private float poseMatchHelperDuration = 10.0f;
        [SerializeField] private TextMeshProUGUI medianTextLeft;
        [SerializeField] private TextMeshProUGUI medianTextRight;
        [SerializeField] private Image progress;
        
        [SerializeField] private TextMeshProUGUI debugGestureDetectionLeft;
        [SerializeField] private TextMeshProUGUI debugGestureDetetectionRight;

        [SerializeField] private Image matchDurationFillLeft;
        [SerializeField] private Image matchDurationFillRight;

        
        // Consts
        private const string BvhSuffix = ".bvh";
   
        private float loopEndTimeLeft => loopStartTimeLeft + sequenceDuration;
        private float loopEndTimeRight => loopStartTimeRight + sequenceDuration;

        private float currentSpeedMultiplier = 1f;
        private float initialSequenceDuration =5;
    

        public string BvhDirectory => Application.streamingAssetsPath+"/" + streamingAssetsSubDirectory+"/";

    
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
        
        
        [Header("Finger Configuration")] 
        [SerializeField] public bool[] left_hand_finger_mask = new bool[5];
        [SerializeField] public bool[] right_hand_finger_mask = new bool[5];
        [SerializeField] public Toggle[] leftFingerToggles;
        [SerializeField] public Toggle[] rightFingerToggles;

        [SerializeField] public Slider[] leftFingerSliders;
        [SerializeField] public Slider[] rightFingerSliders;

        // Private runtime vars
        private bool[] left_hand_compare_mask;
        private bool[] right_hand_compare_mask;
        private float loopStartTimeLeft;
        private float loopStartTimeRight;
        private float matchDurationLeft;
        private float matchDurationRight;
        private List<int> playedFramesLeft;
        private List<int> playedFramesRight;
        private bool PoseMatchHelperStarted=false;
        private float remainingPoseMatchHelperDuration;
        private List<float> mediansLeft;
        private List<float> mediansRight;
        


        // Gesture Module Class Instances    
        private readonly DFKI.GestureModule _leftGestureModule = new();
        private readonly DFKI.GestureModule _rightGestureModule = new();
    
        [Header("Events")]
        // Events
        public UnityEvent<HandGestureParams> GestureCheckChanged = new UnityEvent<HandGestureParams>();
        public UnityEvent<HandGestureParams> AllSequencesPlayedEvent = new UnityEvent<HandGestureParams>();    
        
       
        
        #region Propertys
    
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


        public float SequenceDuration
        {
            get => sequenceDuration;
            set => sequenceDuration = value;
        }

        public float PoseMatchingThresholdRight
        {
            get => poseMatchingThresholdRight;
            set => poseMatchingThresholdRight = value;
        }

        public float PoseMatchingThresholdLeft
        {
            get => poseMatchingThresholdLeft;
            set => poseMatchingThresholdLeft = value;
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
        #endregion
    
        protected override void StartupEnter()
        {
            base.StartupEnter();

            if (leftFingerToggles.Length > 0) {
                leftFingerToggles[0].onValueChanged.AddListener((val) => { ToggleThumb_Left(val); });
                leftFingerToggles[1].onValueChanged.AddListener((val) => { ToggleIndex_Left(val); });
                leftFingerToggles[2].onValueChanged.AddListener((val) => { ToggleMiddle_Left(val); });
                leftFingerToggles[3].onValueChanged.AddListener((val) => { ToggleRing_Left(val); });
                leftFingerToggles[4].onValueChanged.AddListener((val) => { TogglePinky_Left(val); });
            }

            if (rightFingerToggles.Length > 0) {
                rightFingerToggles[0].onValueChanged.AddListener((val) => { ToggleThumb_Right(val); });
                rightFingerToggles[1].onValueChanged.AddListener((val) => { ToggleIndex_Right(val); });
                rightFingerToggles[2].onValueChanged.AddListener((val) => { ToggleMiddle_Right(val); });
                rightFingerToggles[3].onValueChanged.AddListener((val) => { ToggleRing_Right(val); });
                rightFingerToggles[4].onValueChanged.AddListener((val) => { TogglePinky_Right(val); });
            }

            if (autoStart) {
                InitSequence();
                Play(autoStartSequenceLeft,autoStartSequenceRight);
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
                else if (!playAllSequences && loopSingleSequencePlayback) {
                    startTime = Time.time;
                    currentFrame = gestureModule.GetSequenceStartFrameIndex(currentSequence);
                }
                else {
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


        public void StartPoseMatchHelper() {
            analyzePoseMatching = true;
            remainingPoseMatchHelperDuration = poseMatchHelperDuration+2.0f;
            PoseMatchHelperStarted = true;
            mediansLeft = new List<float>();
            mediansRight = new List<float>();
        }

        #region Left Finger Toggles
        public void ToggleThumb_Left(bool val)
        {
            left_hand_finger_mask[0] = val;

            left_hand_compare_mask[45] = val;
            left_hand_compare_mask[46] = val;
            left_hand_compare_mask[47] = val;

            left_hand_compare_mask[48] = val;
            left_hand_compare_mask[49] = val;
            left_hand_compare_mask[50] = val;

            left_hand_compare_mask[51] = val;
            left_hand_compare_mask[52] = val;
            left_hand_compare_mask[53] = val;

            if (leftFingerSliders.Length>0)
            {
                leftFingerSliders[0].interactable = val;
                leftFingerSliders[0].value = val ? leftFingerSliders[0].value : 0.0f;
            }
        }

        public void ToggleIndex_Left(bool val)
        {
            left_hand_finger_mask[1] = val;

            left_hand_compare_mask[9] = val;
            left_hand_compare_mask[10] = val;
            left_hand_compare_mask[11] = val;

            left_hand_compare_mask[12] = val;
            left_hand_compare_mask[13] = val;
            left_hand_compare_mask[14] = val;

            left_hand_compare_mask[15] = val;
            left_hand_compare_mask[16] = val;
            left_hand_compare_mask[17] = val;
            if (leftFingerSliders.Length>0)
            {
            leftFingerSliders[1].interactable = val;
            leftFingerSliders[1].value = val ? leftFingerSliders[1].value : 0.0f;
            }
        }

        public void ToggleMiddle_Left(bool val)
        {
            left_hand_finger_mask[2] = val;

            left_hand_compare_mask[18] = val;
            left_hand_compare_mask[19] = val;
            left_hand_compare_mask[20] = val;

            left_hand_compare_mask[21] = val;
            left_hand_compare_mask[22] = val;
            left_hand_compare_mask[23] = val;

            left_hand_compare_mask[24] = val;
            left_hand_compare_mask[25] = val;
            left_hand_compare_mask[26] = val;
            if (leftFingerSliders.Length>0)
            {
            leftFingerSliders[2].interactable = val;
            leftFingerSliders[2].value = val ? leftFingerSliders[2].value : 0.0f;
            }
        }

        public void ToggleRing_Left(bool val)
        {
            left_hand_finger_mask[3] = val;

            left_hand_compare_mask[36] = val;
            left_hand_compare_mask[37] = val;
            left_hand_compare_mask[38] = val;

            left_hand_compare_mask[39] = val;
            left_hand_compare_mask[40] = val;
            left_hand_compare_mask[41] = val;

            left_hand_compare_mask[42] = val;
            left_hand_compare_mask[43] = val;
            left_hand_compare_mask[44] = val;
            if (leftFingerSliders.Length>0)
            {
            leftFingerSliders[3].interactable = val;
            leftFingerSliders[3].value = val ? leftFingerSliders[3].value : 0.0f;
            }
        }

        public void TogglePinky_Left(bool val)
        {
            left_hand_finger_mask[4] = val;

            left_hand_compare_mask[27] = val;
            left_hand_compare_mask[28] = val;
            left_hand_compare_mask[29] = val;

            left_hand_compare_mask[30] = val;
            left_hand_compare_mask[31] = val;
            left_hand_compare_mask[32] = val;

            left_hand_compare_mask[33] = val;
            left_hand_compare_mask[34] = val;
            left_hand_compare_mask[35] = val;
            
            if (leftFingerSliders.Length>0) {
                leftFingerSliders[4].interactable = val;
                leftFingerSliders[4].value = val ? leftFingerSliders[4].value : 0.0f;
            }
        }
        #endregion

        #region Right Finger Toggles

        public void ToggleThumb_Right(bool val)
        {
            right_hand_finger_mask[0] = val;

            right_hand_compare_mask[45] = val;
            right_hand_compare_mask[46] = val;
            right_hand_compare_mask[47] = val;

            right_hand_compare_mask[48] = val;
            right_hand_compare_mask[49] = val;
            right_hand_compare_mask[50] = val;

            right_hand_compare_mask[51] = val;
            right_hand_compare_mask[52] = val;
            right_hand_compare_mask[53] = val;
            
            if (rightFingerSliders.Length>0) {
                rightFingerSliders[0].interactable = val;
                rightFingerSliders[0].value = val ? rightFingerSliders[0].value : 0.0f;
            }
        }

        public void ToggleIndex_Right(bool val)
        {
            right_hand_finger_mask[1] = val;

            right_hand_compare_mask[9] = val;
            right_hand_compare_mask[10] = val;
            right_hand_compare_mask[11] = val;

            right_hand_compare_mask[12] = val;
            right_hand_compare_mask[13] = val;
            right_hand_compare_mask[14] = val;

            right_hand_compare_mask[15] = val;
            right_hand_compare_mask[16] = val;
            right_hand_compare_mask[17] = val;

            if (rightFingerSliders.Length>0) {
                rightFingerSliders[1].interactable = val;
                rightFingerSliders[1].value = val ? rightFingerSliders[1].value : 0.0f;
            }
        }

        public void ToggleMiddle_Right(bool val)
        {
            right_hand_finger_mask[2] = val;

            right_hand_compare_mask[18] = val;
            right_hand_compare_mask[19] = val;
            right_hand_compare_mask[20] = val;

            right_hand_compare_mask[21] = val;
            right_hand_compare_mask[22] = val;
            right_hand_compare_mask[23] = val;

            right_hand_compare_mask[24] = val;
            right_hand_compare_mask[25] = val;
            right_hand_compare_mask[26] = val;
            if (rightFingerSliders.Length>0)
            {
                rightFingerSliders[2].interactable = val;
                rightFingerSliders[2].value = val ? rightFingerSliders[2].value : 0.0f;
            }
        }

        public void ToggleRing_Right(bool val)
        {
            right_hand_finger_mask[3] = val;

            right_hand_compare_mask[36] = val;
            right_hand_compare_mask[37] = val;
            right_hand_compare_mask[38] = val;

            right_hand_compare_mask[39] = val;
            right_hand_compare_mask[40] = val;
            right_hand_compare_mask[41] = val;

            right_hand_compare_mask[42] = val;
            right_hand_compare_mask[43] = val;
            right_hand_compare_mask[44] = val;

            if (rightFingerSliders.Length>0)
            {

                rightFingerSliders[3].interactable = val;
                rightFingerSliders[3].value = val ? rightFingerSliders[3].value : 0.0f;

            }
        }

        public void TogglePinky_Right(bool val)
        {
            right_hand_finger_mask[4] = val;

            right_hand_compare_mask[27] = val;
            right_hand_compare_mask[28] = val;
            right_hand_compare_mask[29] = val;

            right_hand_compare_mask[30] = val;
            right_hand_compare_mask[31] = val;
            right_hand_compare_mask[32] = val;

            right_hand_compare_mask[33] = val;
            right_hand_compare_mask[34] = val;
            right_hand_compare_mask[35] = val;

            if (rightFingerSliders.Length>0) {
                rightFingerSliders[4].interactable = val;
                rightFingerSliders[4].value = val ? rightFingerSliders[4].value : 0.0f;
            }

        }
        

        #endregion

        private void Update()
        {
            ProcessFrame(_leftGestureModule, ref currentFrameLeft, ref currentSequenceLeft, leftExpertHand, ref loopStartTimeLeft, loopEndTimeLeft, ref isPlayingLeft, ref playedFramesLeft, ref normalizedProgressLeft, ref normalizedProgressTotalLeft, Hand.Left);
            ProcessFrame(_rightGestureModule, ref currentFrameRight, ref currentSequenceRight, rightExpertHand, ref loopStartTimeRight, loopEndTimeRight, ref isPlayingRight, ref playedFramesRight, ref normalizedProgressRight, ref normalizedProgressTotalRight, Hand.Right);
            
            
            if (Application.isEditor) {
            
                if (Input.GetKeyDown(KeyCode.P)) {
                    Pause();
                }

                if (Input.GetKeyDown(KeyCode.R)) {
                    Resume();
                }


                if (PoseMatchHelperStarted && remainingPoseMatchHelperDuration > 0) {
                    remainingPoseMatchHelperDuration -= Time.deltaTime;
                    if(progress) progress.fillAmount = 1.0f-(remainingPoseMatchHelperDuration/(poseMatchHelperDuration));
                    if (leftHand.IsTracked && isPlayingLeft && (remainingPoseMatchHelperDuration<=poseMatchHelperDuration)) {
                        // get current life user frames from headset
                        //float[] user_frame_data_left = GetFrame(leftRecorder);
                        //var result = _leftGestureModule.GetMaximumDifferenceFromCurrentPose(_leftGestureModule.GetSequenceEndFrameIndex(currentSequenceLeft), user_frame_data_left,left_hand_compare_mask);
                        
                        var result = _leftGestureModule.currentMeanDeviation;
                        if (result > 0.0f) {
                            mediansLeft.Add(result);
                        }

                        float currentMedianLeft = mediansLeft.Sum() / mediansLeft.Count;
                        if(medianTextLeft)medianTextLeft.text = currentMedianLeft.ToString();
                   
                    }

                    if (rightHand.IsTracked && isPlayingRight && (remainingPoseMatchHelperDuration <= poseMatchHelperDuration)) {
                        // get current life user frames from headset
                        //float[] user_frame_data_right = GetFrame(rightRecorder);
                        //var result = _rightGestureModule.GetMaximumDifferenceFromCurrentPose(_rightGestureModule.GetSequenceEndFrameIndex(currentSequenceRight), user_frame_data_right,  right_hand_compare_mask);
                        
                        var result = _rightGestureModule.currentMeanDeviation;
                        if (result > 0.0f) {
                            mediansRight.Add(result);
                        }

                        float currentMedianRight = mediansRight.Sum() / mediansRight.Count;
                        if(medianTextRight) medianTextRight.text = currentMedianRight.ToString();
                    }
                }
            

            }

            if (AnalyzePoseMatching == false)
            {
                if (matchDurationFillLeft)
                {
                    matchDurationFillLeft.transform.parent.gameObject.SetActive(false);
                    matchDurationFillLeft.fillAmount = 0.0f;
                }

                if (matchDurationFillRight)
                {
                    matchDurationFillRight.transform.parent.gameObject.SetActive(false);
                    matchDurationFillRight.fillAmount = 0.0f;
                }
            }

            if (AnalyzePoseMatching) {
                
                AnalyzePoseMatch();
                
                if (Application.isEditor) {
                
                    // Simulate left finished event
                    if (Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        HandGestureParams gestureParams = new HandGestureParams();
                        gestureParams.isMatching = true;
                        gestureParams.side = Hand.Left;
                        gestureParams.sequenceIndex = currentSequenceLeft;
                        gestureParams.passedMinDuration = true;
                        GestureCheckChanged.Invoke(gestureParams);
                    }

                    // Simulate right finished event
                    if (Input.GetKeyDown(KeyCode.Alpha2))
                    {
                        HandGestureParams gestureParams = new HandGestureParams();
                        gestureParams.isMatching = true;
                        gestureParams.side = Hand.Right;
                        gestureParams.sequenceIndex = currentSequenceRight;
                        gestureParams.passedMinDuration = true;
                        GestureCheckChanged.Invoke(gestureParams);
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


        public bool MatchingLeft { private set; get; }
        public bool MatchingRight { private set; get; }
    
        protected void AnalyzePoseMatch() {
       
        
            if (isPlayingLeft) {
                
                // get current life user frames from headset
                float[] user_frame_data_left = GetFrame(leftRecorder);
                
                // Distance check
                bool distanceCheckL;
                if (checkDistance) {
                    Assert.IsNotNull(knotbankCenter,"KnotbankCenter null");
                    Assert.IsNotNull(leftHandAnchor,"leftHandAnchor null");
                    float distanceL = Vector3.Distance(knotbankCenter.position, leftHandAnchor.position);
                    distanceCheckL = checkDistance == false || distanceL <= maxDistance;
                }
                else {
                    distanceCheckL = true;
                }
                
                // compare user frames with end poses
                MatchingLeft = distanceCheckL && _leftGestureModule.SimilarPose(
                        _leftGestureModule.GetSequenceEndFrameIndex(currentSequenceLeft), user_frame_data_left,
                        PoseMatchingThresholdLeft, left_hand_compare_mask);

                if (MatchingLeft) {
                    HandGestureParams gestureParams = new HandGestureParams();
                    gestureParams.isMatching = MatchingLeft;
                    gestureParams.side = Hand.Left;
                    gestureParams.sequenceIndex = currentSequenceLeft;
                    gestureParams.passedMinDuration = matchDurationLeft >= poseMatchingMinDuration;
                    GestureCheckChanged.Invoke(gestureParams);
                }
                
                matchDurationLeft += MatchingLeft ? Time.deltaTime : 0.0f;
               
                // Update UI Left
                if (matchDurationFillLeft)
                {
                    matchDurationFillLeft.transform.parent.gameObject.SetActive(matchDurationLeft<=poseMatchingMinDuration);
                    matchDurationFillLeft.fillAmount = Mathf.Clamp01(matchDurationLeft / poseMatchingMinDuration);
                }
                
                UpdateSliders_Left();
               
                if (debugGestureDetectionLeft) {
                    debugGestureDetectionLeft.text = (Mathf.Round(_leftGestureModule.currentMeanDeviation*100f)/100f).ToString();
                }
               
            }

            if (isPlayingRight)
            {
                
                // get current life user frames from headset
                float[] user_frame_data_right = GetFrame(rightRecorder);
          
                // Distance check
                bool distanceCheckR;
                if (checkDistance) {
                    Assert.IsNotNull(knotbankCenter,"KnotbankCenter null");
                    Assert.IsNotNull(rightHandAnchor,"rightHandAnchor null");
                    float distanceR = Vector3.Distance(knotbankCenter.position, rightHandAnchor.position);
                    distanceCheckR = checkDistance==false || distanceR <= maxDistance;
                }
                else {
                    distanceCheckR = true;
                }
                    
                // compare user frames with end poses
                MatchingRight = distanceCheckR && _rightGestureModule.SimilarPose(_rightGestureModule.GetSequenceEndFrameIndex(currentSequenceRight), user_frame_data_right,
                    PoseMatchingThresholdRight, right_hand_compare_mask);
                        

                if (MatchingRight) {
                    HandGestureParams gestureParams = new HandGestureParams();
                    gestureParams.isMatching = MatchingRight;
                    gestureParams.side = Hand.Right;
                    gestureParams.sequenceIndex = currentSequenceRight;
                    gestureParams.passedMinDuration = matchDurationRight>= poseMatchingMinDuration;
                    GestureCheckChanged.Invoke(gestureParams);
                }
                UpdateSliders_Right();
                
                
                matchDurationRight += MatchingRight ? Time.deltaTime : 0.0f;
                // Update UI Right
                if (matchDurationFillRight)
                {
                    matchDurationFillRight.transform.parent.gameObject.SetActive(matchDurationRight<=poseMatchingMinDuration);
                    matchDurationFillRight.fillAmount = Mathf.Clamp01(matchDurationRight / poseMatchingMinDuration);
                }
              
                if (debugGestureDetetectionRight) {
                    debugGestureDetetectionRight.text =  (Mathf.Round(_rightGestureModule.currentMeanDeviation*100f)/100f).ToString();
                }
               
            }

        }

        public void SetWristWeightsLeft(float x, float y, float z) => _leftGestureModule.SetWristWeights(x, y, z);
        public void SetWristWeightsRight(float x, float y, float z) => _rightGestureModule.SetWristWeights(x, y, z);

        public void SetElbowWeightsLeft(float x, float y, float z) => _leftGestureModule.SetElbowWeights(x, y, z);
        public void SetElboWeightsRight(float x, float y, float z) => _rightGestureModule.SetElbowWeights(x, y, z);
        
        // Get currentDigitDeviation from _leftGestureModule and update the sliders
        private void UpdateSliders_Left()
        {
            if (leftFingerSliders.Length > 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    //leftFingerSliders[i].value = _leftGestureModule.currentDigitDeviation[i] / 100f;
                    //Debug.Log($"Left Slider [{i}]: {leftFingerSliders[i].value}");
                }
            }
        }

        // Get currentDigitDeviation from _rightGestureModule and update the sliders
        private void UpdateSliders_Right()
        {
            if (rightFingerSliders.Length > 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    //rightFingerSliders[i].value = _rightGestureModule.currentDigitDeviation[i] / 100f;
                    //Debug.Log($"Right Slider [{i}]: {rightFingerSliders[i].value}");
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
            matchDurationLeft = 0.0f;
            matchDurationRight = 0.0f;
            MatchingLeft = false;
            MatchingRight = false;
            
            if(matchDurationFillLeft) matchDurationFillLeft.transform.parent.gameObject.SetActive(false);
            if(matchDurationFillRight) matchDurationFillRight.transform.parent.gameObject.SetActive(false);

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
            
            playedFramesLeft = new List<int>();
            playedFramesRight = new List<int>();
            
            // PoseMatching
            matchDurationLeft = 0.0f;
            matchDurationRight = 0.0f;
            MatchingLeft = false;
            MatchingRight = false;

            isPlayingLeft = true;
            isPlayingRight = true;
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
            
            // LEFT HAND
            /*for (int i = 0; i < 6; i++) {
                left_hand_compare_mask[i] = false;
            }*/
            /*
            left_hand_compare_mask[0] = false; // Elbow Pos.x
            left_hand_compare_mask[1] = false; // Elbow Pos.y
            left_hand_compare_mask[2] = false; // Elbow Pos.z
            
            left_hand_compare_mask[3] = false; // Elbow Rot.z 
            left_hand_compare_mask[4] = false; // Elbow Rot.x
            left_hand_compare_mask[5] = false; // Elbow Rot.y
            */
            for (int i = 0; i < left_hand_compare_mask.Length; i++) {
                left_hand_compare_mask[i] = true;
            }
            
            // RIGHT HAND
            right_hand_compare_mask = new bool[_rightGestureModule.GetParameterCount()];
            /*for (int i = 0; i < 6; i++) {
                right_hand_compare_mask[i] = false;
            }*/
            for (int i = 0; i < right_hand_compare_mask.Length; i++) {
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
