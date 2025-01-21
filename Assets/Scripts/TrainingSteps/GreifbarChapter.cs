using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace DFKI.NMY
{
    public enum TrainingPhase
    {
        L0_INTRO,
        L1_KNOTENTECHNIK,
        L2_KNOTENSPANNUNG,
        L3_ZEITMESSUNG
    }
    
    public class GreifbarChapter : ChapterTrainingStep,IKnotbAR
    {
        [Header("GreifbAR Chapter Config")]
        [SerializeField] private LocalizedString chapterTitle;

        [Header("Config")]
        //[SerializeField] private List<BaseTrainingStep> runtimeSteps;
        [SerializeField] private TrainingPhase phase;
        [FormerlySerializedAs("targeTime")]
        [Header("Analysis Config")]
        [SerializeField] private float targetTime = 60f;
        [SerializeField] private bool analysisChapter = true;

        
            
        public TrainingPhase Phase { get=>phase; set=>phase=value; }
        public bool AffectTimer { get; set; }
        
        public float TargetTime
        {
            get => targetTime;
            set => targetTime = value;
        }

        public bool AnalysisChapter
        {
            get => analysisChapter;
            set => analysisChapter = value;
        }

        public LocalizedString ChapterTitle
        {
            get => chapterTitle;
            set => chapterTitle = value;
        }

        public List<BaseTrainingStep> customRuntimeSteps = new List<BaseTrainingStep>();


        protected override void Awake()
        {
            if (customRuntimeSteps.Count > 0)
            {
                nextSteps.Clear();
                nextSteps.AddRange(customRuntimeSteps);
            }


            base.Awake();
        }



        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct) {
            await base.PreStepActionAsync(ct);
            
            // TensionMeter activation depending on Level
            GreifbARApp.instance.userInterfaceManager.ActivateTensionMeterByPhase(Phase);
            
            foreach (var step in nextSteps) {
                IKnotbAR knotbARStep = (step as IKnotbAR);
                if (knotbARStep !=null) {
                    knotbARStep.Phase = Phase;
                }
            }
        }
        
        

        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
        }
    }
}