using UnityEngine;
using System.Threading;
using UnityEngine.Events;
using UnityEngine.Serialization;


namespace DFKI.NMY.PoincloudPlayer
{

	public enum PlayState {Playing,Paused,Buffering,Stopped}

	public class PointCloudPlayer : MonoBehaviour
	{

		[Header("Player Config")] 
		[SerializeField] private string pathToSequence;
		[SerializeField] private bool playStream = false;
		[SerializeField] private bool loopPlay = true;
		[SerializeField] int numberOfFramesBufferedBeforePlay = 200;
		
		[Header("References")] [SerializeField]
		private PointCloudRenderer pointCloudRenderer;

		
		[Range(1,100)][SerializeField] private float fps = 30f;
		private float playNextFrameTime;

		
		private bool readerInitialized = false;
		public PlayState status;
		
		// private helper vars
		private int upperBufferSize = 100;
		private int lowerBufferSize = 10;
		private int currentFrameIndex = 0;
		private float millisElapsedInCurrentPlaySequence = 0f;
		private bool buffering;
		private bool runReaderThread;
		private BufferedPointCloudReader bpcReader;
		private Thread readerThread = null;

		// Events
		[Header("Events")] public UnityEvent FinishedStream;

		#region Propertys

		public string PathToSequence
		{
			get => pathToSequence;
			set => pathToSequence = value;
		}

		public float FPS
		{
			get => fps;
			set => fps = value;
		}

		public int CurrentFrameIndex
		{
			get => currentFrameIndex;
			set => currentFrameIndex = value;
		}

		public bool LoopPlay
		{
			get => loopPlay;
			set => loopPlay = value;
		}

		public int GetTotalFrames(){
			if (bpcReader != null) return bpcReader.nFrames;
			return 0;
		} 
		
		#endregion

		void Start(){
			upperBufferSize = lowerBufferSize + numberOfFramesBufferedBeforePlay;
			status = PlayState.Stopped;
			pointCloudRenderer.gameObject.SetActive(false);
		}

		
		public void SetupReaderAndPCManager()
		{

			StopThread();
			bpcReader = new BufferedPointCloudReader(pathToSequence + "/");
			bpcReader.ReadConfig();

			readerThread = new Thread(ReaderThreadRunner);
			readerThread.Start();
			
			runReaderThread = true;
			readerInitialized = true;

		}

		public void StopThread()
		{
			playStream = false;
			runReaderThread = false;
			readerInitialized = false;
			if (readerThread != null){
				readerThread.Abort();
				readerThread = null;
			}
		
			status = PlayState.Stopped;
			pointCloudRenderer.gameObject.SetActive(false);
		}

		public void Play()
		{
			if (!readerInitialized){
				SetupReaderAndPCManager();
			}
			playStream = true;
		}

		public void Pause()
		{
			playStream = false;
			status = PlayState.Paused;
		}

		public void Restart()
		{
			currentFrameIndex = 0;
			playNextFrameTime = 0;
			millisElapsedInCurrentPlaySequence = 0;
			playStream = true;
		}

		public void NextFrame()
		{
			// only allowed in next frame
			if ((playStream || status.Equals(PlayState.Paused)) && ( currentFrameIndex < (bpcReader.nFrames - 1))){
				currentFrameIndex++;
				RenderCurrentFrame();
			} 
		}

		void ReaderThreadRunner()
		{
			while (runReaderThread)
			{
				bpcReader.ReadFrame();

				if (bpcReader.nFramesRead == bpcReader.nFrames)
				{
					runReaderThread = false;
				}
			}
		}


		protected void RenderCurrentFrame()
		{
			// render frames only if point data is available
			if (bpcReader.frameBuffer[currentFrameIndex].frameDataIsAvailable){
				// renderer sends vertices and colors to GPU
				pointCloudRenderer.UpdateMesh(bpcReader.frameBuffer[currentFrameIndex].frameData.frameVertices,
					bpcReader.frameBuffer[currentFrameIndex].frameData.frameColors, Matrix4x4.identity);
			}
		}

		void Update()
		{

			if (bpcReader == null) return;

			int bufferCount = bpcReader.nFramesRead - currentFrameIndex;
			bool allFramesRead = bpcReader.nFrames == bpcReader.nFramesRead;

			if (allFramesRead)
			{
				buffering = false;
			}
			else if (buffering && upperBufferSize <= bufferCount)
			{
				buffering = false;
			}
			else if (!buffering && (lowerBufferSize >= bufferCount))
			{
				buffering = true;
				status = PlayState.Buffering;
			}

			if (!playStream || buffering) return;
			pointCloudRenderer.gameObject.SetActive(true);
			status = PlayState.Playing;
			millisElapsedInCurrentPlaySequence += Time.deltaTime * 1000;

				// Render new pointCloudFrame only if enough time has passed
				if (millisElapsedInCurrentPlaySequence >= playNextFrameTime)
				{
					RenderCurrentFrame();
					currentFrameIndex++;
					playNextFrameTime += 1000 / fps;

				}

				if (currentFrameIndex > bpcReader.nFrames - 1)
				{
					currentFrameIndex = 0;
					playNextFrameTime = 0;
					millisElapsedInCurrentPlaySequence = 0;
					FinishedStream.Invoke();
					if (loopPlay)
					{
						Restart();
					}
					else
					{
						StopThread();
					}
				}
			


		}


	}
}