using System.IO;
using BuildingVolumes.Streaming;
using NMY;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
namespace DFKI.NMY
{
    public class GreifbarGeometrySequencePlayer : MonoBehaviour {
        
        [SerializeField] private bool useReducedSpeed = false;
        private float currentSpeedMultiplier = 1f;
        
        // Events
        [Header("Events")] public UnityEvent FinishedStream = new UnityEvent();
        
        GeometrySequenceStream stream;

        [SerializeField]
        string relativePath = "";
        [SerializeField]
        string absolutePath = "";
        [SerializeField]
        public GeometrySequenceStream.PathType pathRelation;

        //[SerializeField] bool playAtStart = true;
        [SerializeField]
        bool loopPlay = true;
        [SerializeField]
        float targetPlaybackFPS = 30;

        float modifiedFPS = 30;

        float playbackTimeMs = 0;
        bool play = false;


        [SerializeField] private float fadeInDuration = 1.0f;
        [Header("Loop End Pose Visualization")]
        [SerializeField] [Range(0f,1.0f)] private float endPoseFadeInPoint = 0.9f;
        [SerializeField] private float endPoseFreeze = 3.0f;
        [SerializeField] private float endPoseFadeOut = 0.5f;
        [SerializeField] private MeshGesturePreviewManager previewManager;
        
        public string RelativePath
        {
            get => relativePath;
            set => relativePath = value;
        }

        public bool LoopPlay
        {
            get => loopPlay;
            set => loopPlay = value;
        }

        [ContextMenu("Toggle Speed")]
        public void ToggleSpeed(bool useDefault=false)
        {
            useReducedSpeed = !useDefault && !useReducedSpeed;
            currentSpeedMultiplier = useReducedSpeed ? 0.5f : 1.0f;
            if (IsPlaying())
            {
                Pause();
                Play();
            }
        }
        

        // Start is called before the first frame update
        void Start()
        {
            SetupGeometryStream();
            Assert.IsNotNull(previewManager,"previewManager != null");
        }

        public void SetupGeometryStream()
        {
            // Add a Geometry Sequence Stream if there is non already existing on this gameobject
            if (stream == null)
            {
                stream = gameObject.GetComponent<GeometrySequenceStream>();
                if (stream == null)
                    stream = gameObject.AddComponent<GeometrySequenceStream>();
                stream.SetupMaterials();
            }
        }
        
        
        private float freezeTime;
        private float fadeInTime;
        private float fadeOutTime;
        private float minPointSize = 0.0000f;
        [SerializeField] private float maxPointSize = 0.0016f;
        private MeshRenderer cloudMeshRenderer;
        private void Update()
        {
            if (cloudMeshRenderer == null)
            {
                cloudMeshRenderer = stream.meshRoot.GetComponentInChildren<MeshRenderer>();
            }

            
            if (play)
            {
                
                
                // Sync ArrowPreview if available
                if (previewManager && previewManager.ArrowActive()) {
                    
                    previewManager.currentArrowPreview.SetNormalizedTime(GetCurrentTime()/GetTotalTime());
                }

               
                if (freezeTime>0)
                {
                    fadeOutTime = (fadeOutTime-Time.deltaTime)>=0.0f?fadeOutTime-Time.deltaTime:0.0f;
                    freezeTime -= Time.deltaTime;
                    
                    float normalizedFadeOut = 1f-fadeOutTime/endPoseFadeOut;
                    cloudMeshRenderer.sharedMaterial.SetFloat("_PointSize",Mathf.Lerp(maxPointSize,minPointSize,normalizedFadeOut));
                    
                    if (freezeTime <= 0.0f)
                    {
                        if (previewManager) {
                            previewManager.FadeOutCurrent();
                        }
                        freezeTime = 0.0f;
                        fadeInTime = fadeInDuration;
                    }
                    
                }
                else {

                    if (fadeInTime > 0)
                    {
                        float normalizedFadeIn = 1f-fadeInTime/fadeInDuration;
                        cloudMeshRenderer.sharedMaterial.SetFloat("_PointSize",Mathf.Lerp(minPointSize,maxPointSize,normalizedFadeIn));
                    }
                    else
                    {
                        cloudMeshRenderer.sharedMaterial.SetFloat("_PointSize", maxPointSize);
                    }
                    
                    fadeInTime = (fadeInTime-Time.deltaTime)>=0.0f?fadeInTime-Time.deltaTime:0.0f;
                    playbackTimeMs += Time.deltaTime * 1000;
                }
                if (GetCurrentTime() >= GetTotalTime())
                {
                    GoToTime(0);
                    playbackTimeMs = 0f;
                    FinishedStream.Invoke();
                    if (!loopPlay)
                    {
                        Pause();
                    }
                    else
                    {
                        
                        freezeTime = endPoseFreeze;
                        fadeOutTime = endPoseFadeOut;
                        
                        if (previewManager  && previewManager.PreviewActive()) {
                            previewManager.FadeInCurrent();
                        }
                        else
                        {
                            freezeTime = 0.5f;
                        }
                    }
                }

                stream.UpdateFrame(playbackTimeMs);
            }
            
        }

        //+++++++++++++++++++++ PLAYBACK API ++++++++++++++++++++++++

        /// <summary>
        /// Load a .ply sequence (and optionally textures) from the path, and start playback if autoplay is enabled.
        /// Returns false when sequence could not be loaded, see Unity Console output for details in this case
        /// </summary>
        /// <param name="path"></param>
        public bool LoadSequence(string path, GeometrySequenceStream.PathType relativeTo, float playbackFPS)
        {
            if (path.Length < 1)
                return false;

            relativePath = path;
            pathRelation = relativeTo;
            modifiedFPS = playbackFPS;
            play = false;

            //Set the correct absolute path depending on the files location
            if (pathRelation == GeometrySequenceStream.PathType.RelativeToDataPath)
                absolutePath = Path.Combine(Application.dataPath, relativePath);

            if (pathRelation == GeometrySequenceStream.PathType.RelativeToStreamingAssets)
                absolutePath = Path.Combine(Application.streamingAssetsPath, relativePath);

            if (pathRelation == GeometrySequenceStream.PathType.RelativeToPersistentDataPath)
                absolutePath = Path.Combine(Application.persistentDataPath, relativePath);

            if (pathRelation == GeometrySequenceStream.PathType.AbsolutePath)
                absolutePath = relativePath;

            bool sucess = stream.ChangeSequence(absolutePath, playbackFPS);

            if (sucess)
                PlayFromStart();

            return sucess;
        }

        [ContextMenu("Play")]
        /// <summary>
        /// Start Playback from the current location
        /// </summary>
        public void Play()
        {
            fadeInTime = fadeInDuration;
            LoadSequence(relativePath, pathRelation, targetPlaybackFPS * currentSpeedMultiplier);
            
        }

        /// <summary>
        /// Pause current playback
        /// </summary>
        public void Pause() {
            play = false;
        }

        public void Stop() {
            play = false;
            stream.CleanupMeshAndTexture();
        }
        
        /// <summary>
        /// Activate or deactivate looped playback
        /// </summary>
        /// <param name="enabled"></param>
        public void SetLoopPlay(bool enabled)
        {
            loopPlay = enabled;
        }


        /// <summary>
        /// Seeks to the start of the sequence and then starts playback
        /// </summary>
        public void PlayFromStart()
        {
            if (GoToFrame(0))
            {
                play = true;
            }

        }

        /// <summary>
        /// Goes to a specific frame. Use GetTotalFrames() to check how many frames the clip contains
        /// </summary>
        /// <param name="frame"></param>
        public bool GoToFrame(int frame)
        {
            if(stream != null)
            {
                float time = (frame * stream.targetFrameTimeMs) / 1000;
                return GoToTime(time);
            }

            return false;
        }

        /// <summary>
        /// Goes to a specific time in  a clip. The time is dependent on the framerate e.g. the same clip at 30 FPS is twice as long as at 60 FPS.
        /// </summary>
        /// <param name="timeInSeconds"></param>
        /// <returns></returns>
        public bool GoToTime(float timeInSeconds)
        {
            if (timeInSeconds < 0 || timeInSeconds > GetTotalTime())
                return false;

            playbackTimeMs = timeInSeconds * 1000;

            if (!play)
            {
                stream.UpdateFrame(playbackTimeMs);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Gets the absolute path to the folder containing the sequence
        /// </summary>
        /// <returns></returns>
        public string GetSequencePath()
        {
            return stream.pathToSequence;
        }

        /// <summary>
        /// Is the current clip playing?
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            return play;
        }

        /// <summary>
        /// Is looped playback enabled?
        /// </summary>
        /// <returns></returns>
        public bool GetLoopingEnabled()
        {
            return loopPlay;
        }

        /// <summary>
        /// At which frame is the playback currently?
        /// </summary>
        /// <returns></returns>
        public int GetCurrentFrameIndex()
        {
            if (stream != null)
                return stream.currentFrameIndex;
            return -1;
        }

        /// <summary>
        /// At which time is the playback currently in seconds?
        /// Note that the time is dependent on the framerate e.g. the same clip at 30 FPS is twice as long as at 60 FPS.
        /// </summary>
        /// <returns></returns>
        public float GetCurrentTime()
        {
            return playbackTimeMs / 1000;
        }

        /// <summary>
        /// How many frames are there in total in the whole sequence?
        /// </summary>
        /// <returns></returns>
        public int GetTotalFrames()
        {
            if(stream != null)
                if (stream.bufferedReader != null)
                    return stream.bufferedReader.totalFrames;
            return -1;
        }

        /// <summary>
        /// How long is the sequence in total?
        /// Note that the time is dependent on the framerate e.g. the same clip at 30 FPS is twice as long as at 60 FPS.
        /// </summary>
        /// <returns></returns>
        public float GetTotalTime()
        {
            return GetTotalFrames() / GetTargetFPS();
        }

        /// <summary>
        /// The target fps is the framerate we _want_ to achieve in playback. However, this is not guranteed, if system resources
        /// are too low. Use GetActualFPS() to see if you actually achieve this framerate
        /// </summary>
        /// <returns></returns>
        public float GetTargetFPS()
        {
            return modifiedFPS;
        }

        /// <summary>
        /// What is the actual current playback framerate? If the framerate is much lower than the target framerate,
        /// consider reducing the complexity of your sequence, and don't forget to disable any V-Sync (VSync, FreeSync, GSync) methods!
        /// </summary>
        /// <returns></returns>
        public float GetActualFPS()
        {
            if(stream != null)
                return stream.smoothedFPS;
            return -1;
        }

        /// <summary>
        /// Check if there have been framedrops since you last checked this function
        /// Too many framedrops mean the system can't keep up with the playback
        /// and you should reduce your Geometric complexity or framerate
        /// </summary>
        /// <returns></returns>
        public bool GetFrameDropped()
        {
            if(stream != null)
            {
                bool dropped = stream.frameDropped;
                stream.frameDropped = false;
                return dropped;
            }

            return false;
        }
    }

}

        
        
    
