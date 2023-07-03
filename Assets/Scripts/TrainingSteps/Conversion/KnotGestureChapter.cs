using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DFKI.NMY;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;

public class KnotGestureChapter : ChapterTrainingStep {
    
    
    [Header("Knot Scenario Config")]
    [SerializeField] private string bvhFileLeft = ""; 
    [SerializeField] private string bvhFileRight = "";

    
    
    // PRE STEP
    protected override async UniTask PreStepActionAsync(CancellationToken ct) {
        GestureSequencePlayer.instance.LeftHandBvhFile = bvhFileLeft;
        GestureSequencePlayer.instance.RightHandBvhFile = bvhFileRight;
        GestureSequencePlayer.instance.InitSequence();
        await base.PreStepActionAsync(ct);
        
    }

    // POST STEP
    protected override async UniTask PostStepActionAsync(CancellationToken ct) {
        await base.PostStepActionAsync(ct);
    }
    
    
}
