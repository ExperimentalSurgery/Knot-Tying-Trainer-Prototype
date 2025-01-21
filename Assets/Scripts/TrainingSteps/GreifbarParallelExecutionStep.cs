using NMY.VirtualRealityTraining.Steps;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DFKI.NMY
{
    // Override of ParallelExecutionStep for potential extensions in future releases
    public class GreifbarParallelExecutionStep : ParallelStepFixed,IKnotbAR
    {

        //[SerializeField] private List<TrainingPhase> skipPhases = new List<TrainingPhase>();

        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            foreach (var step in _parallelSteps)
            {
                IKnotbAR knotbARStep = (step as IKnotbAR);
                if (knotbARStep !=null)
                {
                    knotbARStep.Phase = Phase;
                }
                else
                {
                    //Debug.Log("Not starting with that isChapter");
                }
            }
        }

        protected override async UniTask ClientStepActionAsync(CancellationToken ct)
        {
            await base.ClientStepActionAsync(ct);
        }

        public TrainingPhase Phase { get; set; }
        public bool AffectTimer { get; set; }
    }
}
