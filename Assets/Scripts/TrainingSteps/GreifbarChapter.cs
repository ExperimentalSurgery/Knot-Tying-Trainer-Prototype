using System.Threading;
using Cysharp.Threading.Tasks;
using NMY.VirtualRealityTraining.Steps;

namespace DFKI.NMY
{
    public class GreifbarChapter : ChapterTrainingStep
    {
        
        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct)
        {
            await base.PreStepActionAsync(ct);
        }

        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
        }


    }
}