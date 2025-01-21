using UnityEngine;
using Varjo.XR;
using DFKI_Utilities;
using System.IO;

public class KnotTrackerManager : MonoBehaviour
{
    private bool knotInitialized = false;
    private bool knotTrackingEnabled = false;
    private KnotTracker KnotTracker;
    public GameObject[] blobObjects { get; private set; } // Current 3D representation for the Knot

    public VarjoApiManager varjoApiManager;
    public VarjoToUnityTransformationManager varjoToUnityTransformationManager;
    public GameObject trainerBank;
    public GameObject cameraOffset;

    [Tooltip("The VR scene - Needed to switch off VR content in AR-Mode")]
    public GameObject sceneContent;

    public KnotTracker.Settings settings;

    // Start is called before the first frame update
    void Start()
    {
        knotTrackingEnabled = false;
        KnotTracker = new KnotTracker();
        settings = new KnotTracker.Settings(1);

        InitXRMode();
    }

    // Update is called once per frame
    void Update()
    {
        // listen for user input
        bool run_once = false, refine = false;
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("KnotTracker: activating stream");
            varjoApiManager.Enable();
        } 
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.LogWarning("KnotTracker: de-activating stream");
            varjoApiManager.Disable();
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("KnotTracker: activating continuous tracking");
            Enable();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("KnotTracker: de-activating continuous tracking");
            Disable();
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("KnotTracker: initialize/update and move to bank");
            Initialize();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("KnotTracker: run tracking once");
            run_once = true;
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("KnotTracker: refine, i.e. without updating the images");
            refine = true;
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("KnotTracker: move to the trainer bank");
            MoveToBank();
        }
        else if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            GameObject parentObject = new GameObject("Debug Points");
            blobObjects = new GameObject[10];
            for (int i = 0; i < 10; i++)
            {
                GameObject blob = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), parentObject.transform);
                blob.transform.position = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                blobObjects[i] = blob;
            }
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("KnotTracker: save all images");
            KnotTracker.UpdateImages(varjoApiManager.leftTexture, varjoApiManager.rightTexture);
            Texture2D[] images = KnotTracker.PrintAllBlobs(ref varjoApiManager.leftTexture, ref varjoApiManager.rightTexture);

            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null)
                {
                    byte[] bytes = images[i].EncodeToPNG();
                    File.WriteAllBytes(Application.dataPath + "/../Output/knottracker_image_"+i+".png", bytes);
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("KnotTracker: update model colors");
            KnotTracker.UpdateImages(varjoApiManager.leftTexture, varjoApiManager.rightTexture);
            KnotTracker.BackProjectColors();
            UpdateBlobObjectsColors();
        }

        // proceed with updates
        if (varjoApiManager.IsEnabled() && knotInitialized)
        {
            if (knotTrackingEnabled)
            {
                KnotTracker.UpdateImages(varjoApiManager.leftTexture, varjoApiManager.rightTexture);
                KnotTracker.EstimateNextPose();
            }
            else if (run_once)
            {
                KnotTracker.UpdateImages(varjoApiManager.leftTexture, varjoApiManager.rightTexture);
                KnotTracker.EstimatePose();
            }
            else if (refine)
            {
                KnotTracker.EstimatePose();
            }
            UpdateBlobObjects(GetCurrentUnityPosition());
        }
    }

    private Vector3[] GetCurrentUnityPosition()
    {
        Vector3[] varjoPositions = KnotTracker.GetCurrentPosition();
        Vector3[] unityPositions = new Vector3[varjoPositions.Length];

        // update their positions for the Unity interface
        for (int i = 0; i < varjoPositions.Length; i++)
            unityPositions[i] = varjoToUnityTransformationManager.fromVarjoToUnityTransformation.MultiplyPoint3x4(varjoPositions[i]);

        return unityPositions;
    }

    private void InitXRMode()
    {
        sceneContent.SetActive(false);
        VarjoRendering.SetOpaque(false);
        VarjoMixedReality.StartRender();
        Camera camera = Camera.main;
        UnityEngine.Color solid_black = new(0, 0, 0, 0);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = solid_black;
    }

    private void InitBlobObjects()
    {
        // re-initialize 3D blob objects
        if (blobObjects != null)
        {
            for (var i = 0; i < blobObjects.Length; i++)
                Destroy(blobObjects[i]);
        }
        
        Vector3[] positions = GetCurrentUnityPosition();
        UnityEngine.Color[] colors = KnotTracker.GetCurrentColors();
        blobObjects = new GameObject[settings.N_blobs];

        for (var i = 0; i < settings.N_blobs; i++)
            blobObjects[i] = UnityUtils.CreateNewGameObject("blob_" + i, positions[i], colors[i], true, cameraOffset, settings.radius);
    }

    private void UpdateBlobObjectsColors()
    {
        UnityEngine.Color[] colors = KnotTracker.GetCurrentColors();

        for (var i = 0; i < blobObjects.Length; i++)
        {
            GameObject sphere = blobObjects[i];
            
            var renderer = sphere.GetComponent<Renderer>();
            renderer.material.SetColor("_Color", colors[i]);
        }
    }

    private void UpdateBlobObjects(Vector3[] positions)
    {
        if (positions != null && positions.Length == blobObjects.Length)
        {
            for (var i = 0; i < blobObjects.Length; i++)
            {
                GameObject sphere = blobObjects[i];
                sphere.transform.localPosition = positions[i];
            }
        }
    }

    public void Enable()
    {
        knotTrackingEnabled = true;
    }

    public void Disable()
    {
        knotTrackingEnabled = false;
    }

    private void Initialize()
    {
        // Function to initialize the knot tracker or to reset it
        // The stream must be enabled: access to the camera parameters is required

        if (varjoApiManager.IsEnabled())
        {
            // Important: update settings considering any changes made in the interface
            settings.Update();

            // initialize the knot tracker
            KnotTracker.Initialize(ref varjoApiManager.cameraLeft, ref varjoApiManager.cameraRight, settings);

            // initialize the 3D visualization
            InitBlobObjects();

            // move the straight knot to the trainer bank
            MoveToBank();

            knotInitialized = true;
        }
    }

    private void MoveToBank()
    {
        // move the straight knot to the trainer bank

        if (knotInitialized)
        {
            if (trainerBank != null)
            {
                Vector3 p = trainerBank.transform.localPosition;
                p = varjoToUnityTransformationManager.fromUnityToVarjoTransformation.MultiplyPoint3x4(p);
                KnotTracker.MoveTo(p);
                UpdateBlobObjects(GetCurrentUnityPosition());
            }
            else
            {
                Debug.LogError("Error: The trainerbank was not detected!");
            }
        }
    }

    void OnApplicationQuit()
    {
        Disable();
    }
}
