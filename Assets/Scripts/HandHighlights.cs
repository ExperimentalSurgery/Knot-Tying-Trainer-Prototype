using System;
using System.Collections;
using System.Collections.Generic;
using NMY.VirtualRealityTraining;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;

namespace DFKI.NMY
{
    
    [DisallowMultipleComponent]
    public class HandHighlights : MonoBehaviour {

        [SerializeField] private BaseTrainingStep parentStep;
        [SerializeField] private List<FingerHighlightContainer> highlights = new List<FingerHighlightContainer>();

        [SerializeField] private bool preventResetOnComplete = false;
        
        #region Migration Stuff
        private void Reset() {
            parentStep = GetComponent<BaseTrainingStep>();
            highlights = new List<FingerHighlightContainer>();
        }
        #endregion


        private void OnEnable() {
            parentStep.OnStepStarted -= OnParentStepStarted;
            parentStep.OnStepStarted += OnParentStepStarted;
            parentStep.OnStepCompleted -= OnParentStepCompleted;
            parentStep.OnStepCompleted += OnParentStepCompleted;
        }

        private void OnDisable() {
            parentStep.OnStepStarted -= OnParentStepStarted;
            parentStep.OnStepCompleted -= OnParentStepCompleted;
        }

        private void OnParentStepStarted(object sender, BaseTrainingStepEventArgs e) {
            Debug.Log("HandHighlights TriggerHighlights "+gameObject.name);
            foreach (var highlightConfig in highlights) {
                UserInterfaceManager.instance.FingerHighlight(highlightConfig);
            }
        }
        
        private void OnParentStepCompleted(object sender, BaseTrainingStepEventArgs e) {
            // Prevent Highlighter with no highlights to do anything at all...
            if (highlights.Count > 0 && !preventResetOnComplete) {
                Debug.Log("HandHighlights ResetHighlights "+gameObject.name);
                UserInterfaceManager.instance.ResetFingerHighlights();
            }
        }

    }
}
