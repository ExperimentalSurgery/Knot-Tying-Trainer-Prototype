using System.Threading;
using Cysharp.Threading.Tasks;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;

namespace DFKI.NMY
{
    public class GreifbarTimeOut : TimeoutStep,IKnotbAR
    {
        public TrainingPhase Phase { get; set; }
        public bool AffectTimer { 
            get => _affectTimer;
            set => _affectTimer = value;
        }
        
        [SerializeField] private bool _affectTimer = true;


        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
            Debug.Log("PreStepActionAsync "+gameObject.name);
        }

        protected override async UniTask ClientStepActionAsync(CancellationToken ct)
        {
            await base.ClientStepActionAsync(ct);
            Debug.Log("ClientStepActionAsync "+gameObject.name);
        }
    }
}
