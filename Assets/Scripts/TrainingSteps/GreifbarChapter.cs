using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;
using UnityEngine.UI;

namespace DFKI.NMY
{
    public class GreifbarChapter : ChapterTrainingStep
    {
        
        [SerializeField] private LocalizedString chapterTitle;
        [SerializeField] private Image chapterIcon;

        public LocalizedString ChapterTitle
        {
            get => chapterTitle;
            set => chapterTitle = value;
        }

        public Image ChapterIcon
        {
            get => chapterIcon;
            set => chapterIcon = value;
        }

        // PRE STEP
        protected override async UniTask PreStepActionAsync(CancellationToken ct) {
            await base.PreStepActionAsync(ct);
        }

        // POST STEP
        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
        }


    }
}