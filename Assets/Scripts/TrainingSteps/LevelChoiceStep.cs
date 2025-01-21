using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace DFKI.NMY
{
    public class LevelChoiceStep : GreifbarBaseStep,IKnotbAR {
        
        protected override async UniTask PreStepActionAsync(CancellationToken ct) {
            await base.PreStepActionAsync(ct);
            GreifbARApp.instance.levelChoice.Activate();
            FinishedCriteria = false;
        }

        protected override async UniTask ClientStepActionAsync(CancellationToken ct) {
            try {
                GreifbARApp.instance.levelChoice.levelSwitchTriggered.AddListener(OnLevelSwitchTriggered);
                await base.ClientStepActionAsync(ct);
            }
            catch (OperationCanceledException) {
                RaiseClientStepFinished();
            }
        }

        protected override async UniTask PostStepActionAsync(CancellationToken ct)
        {
            await base.PostStepActionAsync(ct);
            GreifbARApp.instance.levelChoice.Deactivate();
            GreifbARApp.instance.levelChoice.levelSwitchTriggered.RemoveListener(OnLevelSwitchTriggered);
        }

        private void OnLevelSwitchTriggered(int arg0)
        {
            FinishedCriteria = true;
        }
    }
}
