using Cysharp.Threading.Tasks;
using NMY.GoogleCloudTextToSpeech;
using System;
using System.Collections.Generic;
using System.Threading;
using DFKI.NMY;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.Serialization;

#nullable disable
namespace NMY.VirtualRealityTraining.Steps.VirtualAssistant
{

    #if NMY_ENABLE_GOOGLE_CLOUD_TTS
    
    [Serializable]
    public class TrainingPhaseToClip
    {
        [SerializeField]
        public TrainingPhase phase;
        
        [SerializeField]
        public LocalizedTextToSpeechAudioClip clip;
    }
    
    #endif
    
  public class GreifbarTextToSpeechStep : GreifbarBaseStep, IKnotbAR
  {
      
    [SerializeField] private List<TrainingPhase> skipPhases = new List<TrainingPhase>();

    [SerializeField] private KeyCode skipShortcut = KeyCode.V;    
    
    #region Serialized Fields
        
        #if NMY_ENABLE_GOOGLE_CLOUD_TTS
        /// <summary>
        /// The localized text-to-speech audio clip that the virtual assistant should use to speak in this training step.
        /// </summary>
        [Header("Speak Settings")]
        [SerializeField] private LocalizedTextToSpeechAudioClip _localizedTtsAudioClip;
        [SerializeField] private List<TrainingPhaseToClip> runtimeClipOverrides;
        
        public LocalizedTextToSpeechAudioClip GetClipByPhase(TrainingPhase phase)
        {

            foreach (var levelCfg in runtimeClipOverrides)
            {

                if (levelCfg.phase == phase) {
                    return levelCfg.clip;
                }
            }
            
            return  null;

        }
        
#endif


      protected override void Awake()
      {
          base.Awake();
          Assert.IsNotNull(_localizedTtsAudioClip,"LocalizedTTS-reference missing "+this.gameObject.name);
      }

      protected override async UniTask PreStepActionAsync(CancellationToken ct)
      {
          var clipCandidate = GetClipByPhase(Phase);
          if (clipCandidate != null)
          {
              SetLocalizedAudioClip(clipCandidate);
          }
          
          await base.PreStepActionAsync(ct);
      }

      #endregion

         
        protected override async UniTask ClientStepActionAsync(CancellationToken ct)
        {
            try
            {
                if (skipPhases.Contains(Phase))
                {
                    FinishedCriteria = true;
                    await base.ClientStepActionAsync(ct);
                    //Debug.Log("should be skipped");
                    RaiseClientStepFinished();
                }
                else
                {
                    
                    await Speak(_localizedTtsAudioClip, ct);
                    FinishedCriteria = true;
                    await base.ClientStepActionAsync(ct);
                    RaiseClientStepFinished();
                }
            }
            catch (OperationCanceledException)
            {
                GreifbARApp.instance.mainAudioSource.Stop();
                RaiseClientStepFinished();
            }
        }


        void Update() {

            if (stepState != StepState.StepStarted) return;
            
            if (Input.GetKeyDown(skipShortcut))
            {
                
                GreifbARApp.instance.mainAudioSource.Stop();
                FinishedCriteria = true;
                ForceContinue();
            }
            
        }

        public async UniTask Speak(LocalizedTextToSpeechAudioClip audioClip, CancellationToken ct, float audioTime = 0)
        {
#if NMY_ENABLE_GOOGLE_CLOUD_TTS
                    //Debug.Log("Speek "+this.gameObject.name);
                    if (_localizedTtsAudioClip == null)
                    {
                        Debug.LogError($"{GetType()}: LocalizedTextToSpeechAudioClip is null!", this);
                        return;
                    }

                    if (_localizedTtsAudioClip.IsEmpty)
                    {
                        Debug.LogError($"{GetType()}: AudioClip was not found in TableReference " +
                                       $"\"{_localizedTtsAudioClip.TableReference.TableCollectionName}\" for TableEntryReference key " +
                                       $"\"{_localizedTtsAudioClip.TableEntryReference.Key}\"", this);
                        return;
                    }


                    try
                    {
                        await LocalizationSettings.InitializationOperation;
                        var item = await _localizedTtsAudioClip.LoadAssetAsync().ToUniTask(cancellationToken: ct);

                        if (item == null || item.audioClip == null)
                        {
                            Debug.LogError($"{GetType()}: LocalizedTextToSpeechItem or AudioClip could not be loaded!", this);
                            return;
                        }
                        
                        GreifbARApp.instance.mainAudioSource.clip = item.audioClip;
                        GreifbARApp.instance.mainAudioSource.time = Mathf.Min(0, item.GetDuration());
                        GreifbARApp.instance.mainAudioSource.Play();
                        await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0, item.GetDuration() - 0)),
                            cancellationToken: ct);
                    }
                    // Something went completely wrong with the asset in the AssetTable - maybe the asset was deleted from the
                    // file system and could therefore not be loaded?
                    catch (OperationException)
                    {
                        Debug.LogError($"{GetType()}: Could not load LocalizedTextToSpeechAudioClip Asset. ", this);
                    }
                    
#endif

        }

        protected override async UniTask ExecuteMoveToStepAction(CancellationToken ct = default)
        {
            GreifbARApp.instance.mainAudioSource.Stop();
            await base.ExecuteMoveToStepAction(ct);
        }

#if NMY_ENABLE_GOOGLE_CLOUD_TTS
       
        public void SetLocalizedAudioClip(LocalizedTextToSpeechAudioClip audioClip) =>
            _localizedTtsAudioClip = audioClip;
#endif

        protected override string GameObjectPrefixName() => "[VA Speak Step]";
    
}
}

