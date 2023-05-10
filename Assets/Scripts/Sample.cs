using Leap;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using Varjo.XR;
using Debug = UnityEngine.Debug;

public class Sample : MonoBehaviour {

    private bool debug = false;

    [Tooltip("The VR scene - Needed to switch off VR content in AR-Mode")]
    public GameObject sceneContent;

    [Tooltip("GameObject of left expert hand")]
    public GameObject leftExpertHand;
    [Tooltip("GameObject of right expert hand")]
    public GameObject rightExpertHand;

    [Tooltip("GameObject of left user hand")]
    public GameObject leftUserHand;
    [Tooltip("GameObject of left user hand")]
    public GameObject rightUserHand;


    [Tooltip("GameObject of left user elbow. Used for quick access during alignment. Probably obsolete ...")]
    public GameObject leftUserElbow;
    [Tooltip("GameObject of right user elbow. Used for quick access during alignment. Probably obsolete ...")]
    public GameObject rightUserElbow;

    [Tooltip("GameObject of left expert elbow. Used for quick access during alignment. Probably obsolete ...")]
    public GameObject leftExpElbow;
    [Tooltip("GameObject of right expert elbow. Used for quick access during alignment. Probably obsolete ...")]
    public GameObject rightExpElbow;

    [Tooltip("The directory where BVH data will be stored or read from")]
    public string bvhRelativeDirectory = "/../BVHdata/";
    [Tooltip("Filename to read left hand BVH data from - no file suffix")]
    public string leftHandBVHFile;
    [Tooltip("Filename to read left hand BVH data from - no file suffix")]
    public string rightHandBVHFile;

    // counts up every update
    int colorChangeCounter_left = 0;
    int colorChangeCounter_right = 0;
    [Tooltip("Number of updates until color change is possible")]
    public int colorChangePersistenceFrames;

    public BVHRecorder leftRecorder;
    public BVHRecorder rightRecorder;

    [Tooltip("Threshold for pose matching. Good default = 25")]
    public float poseMatchingThreshold;

    public Renderer userHandRenderer_left;
    public Renderer userHandRenderer_right;
    public Material errorMaterial;
    public Material successMaterial;
    public Material handMaterial;

    [Tooltip("Integer factor to slow down BVH animation")]
    public int slowdownFactor;
    private int slowdown = 0;

    private int current_sequence_left = 0;      // indexes the current sequence in an BVH dataset
    private int current_sequence_right = 0;
    private int current_frame_left = 0;         // indexes the current frame of a BVH dataset
    private int current_frame_right = 0;

    private readonly DFKI.GestureModule leftGM = new();
    private readonly DFKI.GestureModule rightGM = new();

    private bool[] left_hand_compare_mask;
    private bool[] right_hand_compare_mask;

    private int simulationStage = 0;
    private const int maxSimulationStages = 2;

    private bool userHandsVisible = true;

    private bool ColorHandLeft(Material mat) {
        if (colorChangeCounter_left < colorChangePersistenceFrames)
            return false; // do not change color while cCC < cCPF
        //Debug.Log("Changing color to " + mat.name);
        userHandRenderer_left.material.CopyPropertiesFromMaterial(mat);
        // reset color change counter
        colorChangeCounter_left = 0;
        return true;
    }
    private bool ColorHandRight(Material mat) {
        if (colorChangeCounter_right < colorChangePersistenceFrames)
            return false; // do not change color while cCC < cCPF
        //Debug.Log("Changing color to " + mat.name);
        userHandRenderer_right.material.CopyPropertiesFromMaterial(mat);
        // reset color change counter
        colorChangeCounter_right = 0;
        return true;
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

    private float[] GetFrame(BVHRecorder recorder) {
        string frame = recorder.getLastFrame();
        if (frame == null)
            return null;
        //Debug.Log("Capture Frame Length: " + frame);
        return ParseFloats(frame);
    }

    public void Align() {
        leftExpElbow.transform.SetPositionAndRotation(leftUserElbow.transform.position, leftUserElbow.transform.rotation);
        rightExpElbow.transform.SetPositionAndRotation(rightUserElbow.transform.position, rightUserElbow.transform.rotation);
    }

    public void AdaptScale() {
        leftExpElbow.transform.localScale = leftUserElbow.transform.lossyScale;
        // needs to flip the x-scale due to flipped coordinate axes ... :-/
        Vector3 localScale = rightUserElbow.transform.lossyScale;
        localScale[0] *= -1;
        rightExpElbow.transform.localScale = localScale;
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

    private void SetupSimulationStage() {
        if (simulationStage == 0) {
            InitVRMode();
            SetVisible(leftUserHand);
            SetVisible(rightUserHand);
            userHandsVisible = true;
            return;
        }

        if (simulationStage == 1) {
            InitXRMode();
            SetInvisible(leftUserHand);
            SetInvisible(rightUserHand);
            userHandsVisible = false;
            return;
        }

        simulationStage = 0;
        SetVisible(leftUserHand);
        SetVisible(rightUserHand);
        userHandsVisible = true;
        InitVRMode();
    }

    void StartupAnimatedGestures() {

        slowdown = 0;

        // load and pre-process the expert BVH files
        leftGM.BVHPreprocessing(Application.dataPath + bvhRelativeDirectory + leftHandBVHFile + ".bvh");
        rightGM.BVHPreprocessing(Application.dataPath + bvhRelativeDirectory + rightHandBVHFile + ".bvh");

        // generate compare masks - i.e. ignore first 6 parameters on both hands
        left_hand_compare_mask = new bool[leftGM.GetParameterCount()];
        for (int i = 0; i < 6; i++) {
            left_hand_compare_mask[i] = false;
        }
        for (int i = 6; i < left_hand_compare_mask.Length; i++) {
            left_hand_compare_mask[i] = true;
        }
        right_hand_compare_mask = new bool[rightGM.GetParameterCount()];
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


        // setup for first sequence
        current_sequence_left = 0;
        current_frame_left = leftGM.GetSequenceStartFrameIndex(current_sequence_left);

        current_sequence_right = 0;
        current_frame_right = rightGM.GetSequenceStartFrameIndex(current_sequence_right);

        // setup material for hands - initially set errorMaterial
        ColorHandLeft(errorMaterial);
        ColorHandRight(errorMaterial);

        // allow immediate color change
        colorChangeCounter_left = colorChangePersistenceFrames;
        colorChangeCounter_right = colorChangePersistenceFrames;

        if (debug) {
            Debug.Log("LEFT HAND DEBUG INFO");
            Debug.Log("Number of sequences: " + leftGM.GetNumberOfSequences());
            for (int i=0; i<leftGM.GetNumberOfSequences(); i++)
                Debug.Log("Sequence " + i + ": Start = " + leftGM.GetSequenceStartFrameIndex(i) + " End = " + leftGM.GetSequenceEndFrameIndex(i));
            Debug.Log("RIGHT HAND DEBUG INFO");
            Debug.Log("Number of sequences: " + rightGM.GetNumberOfSequences());
            for (int i = 0; i < rightGM.GetNumberOfSequences(); i++)
                Debug.Log("Sequence " + i + ": Start = " + rightGM.GetSequenceStartFrameIndex(i) + " End = " + rightGM.GetSequenceEndFrameIndex(i));
        }

    }

    //######################################################################################
    void UpdateAnimatedGestures() {

        // manage user hand color
        colorChangeCounter_left++;
        if (colorChangeCounter_left > colorChangePersistenceFrames) {
            colorChangeCounter_left = colorChangePersistenceFrames;
        }
        colorChangeCounter_right++;
        if (colorChangeCounter_right > colorChangePersistenceFrames) {
            colorChangeCounter_right = colorChangePersistenceFrames;
        }

        // update expert hands
        ApplyBVHFrame(leftGM.GetFrame(current_frame_left), leftExpertHand);
        ApplyBVHFrame(rightGM.GetFrame(current_frame_right), rightExpertHand);


        // get current life user frames from headset
        float[] user_frame_data_left = GetFrame(leftRecorder);
        float[] user_frame_data_right = GetFrame(rightRecorder);

        // compare user frames with end poses
        if (leftGM.SimilarPose(leftGM.GetSequenceEndFrameIndex(current_sequence_left), user_frame_data_left, poseMatchingThreshold, left_hand_compare_mask)) {
            ColorHandLeft(successMaterial);
            SetVisible(leftUserHand);
            current_sequence_left++;
            if (current_sequence_left >= leftGM.GetNumberOfSequences()) {
                current_sequence_left = 0;
                simulationStage++;
                SetupSimulationStage();
            }
            current_frame_left = leftGM.GetSequenceStartFrameIndex(current_sequence_left);
        }
        else {
            if (ColorHandLeft(handMaterial)) {
                // allow for immediate change of color
                colorChangeCounter_left = colorChangePersistenceFrames;
                if (userHandsVisible == false) {
                    SetInvisible(leftUserHand);
                }
            }
        }
        if (current_frame_left >= leftGM.GetSequenceEndFrameIndex(current_sequence_left)) {
            current_frame_left = leftGM.GetSequenceStartFrameIndex(current_sequence_left);
            if (debug) 
                Debug.Log("resetting left loop to: " + current_frame_left);
        }

        if (leftGM.SimilarPose(rightGM.GetSequenceEndFrameIndex(current_sequence_right), user_frame_data_right, poseMatchingThreshold, right_hand_compare_mask)) {
            ColorHandRight(successMaterial);
            SetVisible(rightUserHand);
            current_sequence_right++;
            if (current_sequence_right >= rightGM.GetNumberOfSequences()) {
                current_sequence_right = 0;
                simulationStage++;
                SetupSimulationStage();
            }
            current_frame_right = rightGM.GetSequenceStartFrameIndex(current_sequence_right);
        }
        else {
            if (ColorHandRight(handMaterial)) {
                // allow for immediate change of color
                colorChangeCounter_right = colorChangePersistenceFrames;
                if (userHandsVisible == false) {
                    SetInvisible(rightUserHand);
                }
            }
        }
       if (current_frame_right >= rightGM.GetSequenceEndFrameIndex(current_sequence_right)) {
            current_frame_right = rightGM.GetSequenceStartFrameIndex(current_sequence_right);
            if (debug) 
                Debug.Log("resetting right loop to: " + current_frame_right);
        }

        if (debug)
            Debug.Log("CFL: " + current_frame_left + ", CFR: " + current_frame_right);

        slowdown++;
        if (slowdown > slowdownFactor) {
            slowdown = 0;
            current_frame_left++;
            current_frame_right++;
        }

    }

    //######################################################################################
    void Start() {

        simulationStage = 0;
        SetupSimulationStage();
        StartupAnimatedGestures();

    }

    private void InitXRMode() {
        sceneContent.SetActive(false);
        VarjoRendering.SetOpaque(false);
        VarjoMixedReality.StartRender();
        Camera camera = Camera.main;
        Color solid_black = new(0, 0, 0, 0);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = solid_black;

        SetInvisible(leftUserHand);
        SetInvisible(rightUserHand);
    }

    private void InitVRMode() {
        sceneContent.SetActive(true);
        VarjoRendering.SetOpaque(true);
        VarjoMixedReality.StopRender();
        Camera camera = Camera.main;
        camera.clearFlags = CameraClearFlags.Skybox;

        SetVisible(leftUserHand);
        SetVisible(rightUserHand);
    }

    public void SetInvisible(GameObject obj) {
        Renderer[] lChildRenderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer lRenderer in lChildRenderers) {
            lRenderer.enabled = false;
        }
    }
    public void SetVisible(GameObject obj) {
        Renderer[] lChildRenderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer lRenderer in lChildRenderers) {
            lRenderer.enabled = true;
        }
    }

    //######################################################################################
    // Update is called once per frame
    void Update() {

        if (Input.GetKeyDown(KeyCode.R)) {
            Debug.Log("Resetting");
            simulationStage = 0;
            SetupSimulationStage();
            StartupAnimatedGestures();
        }

        if (Input.GetKeyDown(KeyCode.S)) {
            simulationStage++;
            SetupSimulationStage();
            StartupAnimatedGestures();
        }

        if (Input.GetKeyDown(KeyCode.A)) {
            Debug.Log("Aligning");
            //AlignExpertToUser();
            Align();
            AdaptScale();
        }

        if (Input.GetKeyDown(KeyCode.X)) {
            InitXRMode();
        }

        if (Input.GetKeyDown(KeyCode.Z)) {
            InitVRMode();
        }

        UpdateAnimatedGestures();

    }

}