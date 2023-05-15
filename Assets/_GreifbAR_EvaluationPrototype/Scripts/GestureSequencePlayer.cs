using System;
using NMY;
using UnityEngine;

namespace DFKI.NMY
{
    
public class GestureSequencePlayer : SingletonStartupBehaviour<GestureSequencePlayer>
{
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
    private int currentFrameLeft = 0;
    private int currentFrameRight = 0;
    private bool isPlayingLeft = false;
    private bool isPlayingRight = false;
    private float loopStartTimeLeft;
    private float loopStartTimeRight;
    private float loopEndTimeLeft => loopStartTimeLeft + sequenceDuration;
    private float loopEndTimeRight => loopStartTimeRight + sequenceDuration;
    

    private void Update()
    {
        if (isPlayingLeft)
        {
            ApplyBVHFrame(_leftGestureModule.GetFrame(currentFrameLeft), leftExpertHand);

            if (currentFrameLeft >= _leftGestureModule.GetSequenceEndFrameIndex(currentSequenceLeft)) {
                currentSequenceLeft++;
                loopStartTimeLeft = Time.time;
                currentFrameLeft = _leftGestureModule.GetSequenceStartFrameIndex(currentSequenceLeft);
            }
            if (currentSequenceLeft >= _leftGestureModule.GetNumberOfSequences()-1) {
                isPlayingLeft = false;
            }
          
            float normalizedProgress = (Time.time - loopStartTimeLeft) / (loopEndTimeLeft - loopStartTimeLeft);
            currentFrameLeft = _leftGestureModule.GetSequenceStartFrameIndex(currentSequenceLeft) + (int)(_leftGestureModule.GetSequenceLength(currentSequenceLeft) * normalizedProgress);
            
        }

        if (isPlayingRight)
        {
            ApplyBVHFrame(_rightGestureModule.GetFrame(currentFrameRight), rightExpertHand);
            if (currentFrameRight >= _rightGestureModule.GetSequenceEndFrameIndex(currentSequenceRight)) {
                currentSequenceRight++;
                loopStartTimeRight = Time.time;
                currentFrameRight = _rightGestureModule.GetSequenceStartFrameIndex(currentSequenceRight);
            }
            if (currentSequenceRight >= _rightGestureModule.GetNumberOfSequences()-1) {
                isPlayingRight = false;
            }
            float normalizedProgress = (Time.time - loopStartTimeRight) / (loopEndTimeRight - loopStartTimeRight);
            currentFrameRight = _rightGestureModule.GetSequenceStartFrameIndex(currentSequenceRight) + (int)(_rightGestureModule.GetSequenceLength(currentSequenceRight) * normalizedProgress);
        }

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
