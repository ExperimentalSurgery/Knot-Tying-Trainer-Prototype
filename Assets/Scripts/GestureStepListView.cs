using System;
using System.Collections;
using System.Collections.Generic;
using DFKI.NMY;
using DFKI.NMY.TrainingSteps;
using Microsoft.MixedReality.Toolkit.Utilities;
using NMY.VTT.Core;
using UnityEngine;

public class GestureStepListView : MonoBehaviour
{

    [SerializeField] private GridObjectCollection root;
    [SerializeField] private GestureTrainingController training;

    [SerializeField] private GestureStepListEntry entryPrevious;
    [SerializeField] private GestureStepListEntry entryCurrent;
    [SerializeField] private GestureStepListEntry entryNext;
    
    private void Start()
    {
        
        entryPrevious.gameObject.SetActive(false);
        entryCurrent.gameObject.SetActive(false);
        entryNext.gameObject.SetActive(false);
        
        training.EntryActivationEvent -= OnTrainingStarted;
        training.EntryActivationEvent += OnTrainingStarted;

        foreach (var t in training.trainings)
        {
            t.EntryCompletedEvent -= OnStepCompleted;
            t.EntryCompletedEvent += OnStepCompleted;
        }
        
    }

    private void OnStepCompleted(object sender, ListControllerEventArgs e)
    {
        entryPrevious.Highlight(false);
        entryCurrent.Highlight(false);
        entryNext.Highlight(false);
    }

    private void OnTrainingStarted(object sender, ListControllerEventArgs e)
    {
        
    }

    private void Update()
    {
        foreach (var t in training.trainings)
        {
            var scenarioTraining = (t as GestureScenario);
            if (scenarioTraining.isActivated)
            {

                if (scenarioTraining.currentStepIndex > 0 && scenarioTraining.currentStepIndex<scenarioTraining.trainingSteps.Count)
                {
                    entryPrevious.Highlight(false);
                    entryPrevious.gameObject.SetActive(true);
                    
                    entryCurrent.Highlight(true);
                    entryCurrent.gameObject.SetActive(true);
                    
                    entryNext.Highlight(false);
                    entryNext.gameObject.SetActive(true);
                    
                    entryPrevious.SetTitle((scenarioTraining.trainingSteps[scenarioTraining.currentStepIndex-1] as GestureTrainingStep).StepTitle.GetLocalizedString());
                    entryCurrent.SetTitle((scenarioTraining.trainingSteps[scenarioTraining.currentStepIndex] as GestureTrainingStep).StepTitle.GetLocalizedString());
                    entryNext.SetTitle((scenarioTraining.trainingSteps[scenarioTraining.currentStepIndex+1] as GestureTrainingStep).StepTitle.GetLocalizedString());

                }
                else if (scenarioTraining.currentStepIndex > 0 && scenarioTraining.currentStepIndex >= scenarioTraining.trainingSteps.Count)
                {
                    entryPrevious.Highlight(false);
                    entryPrevious.gameObject.SetActive(true);
                    
                    entryCurrent.Highlight(true);
                    entryCurrent.gameObject.SetActive(true);
                    
                    entryNext.Highlight(false);
                    entryNext.gameObject.SetActive(false);
                    
                    entryPrevious.SetTitle((scenarioTraining.trainingSteps[scenarioTraining.currentStepIndex-1] as GestureTrainingStep).StepTitle.GetLocalizedString());
                    entryCurrent.SetTitle((scenarioTraining.trainingSteps[scenarioTraining.currentStepIndex] as GestureTrainingStep).StepTitle.GetLocalizedString());
                }
                else if (scenarioTraining.currentStepIndex == 0)
                {
                    
                    entryPrevious.gameObject.SetActive(false);
                    entryPrevious.Highlight(false);
                    
                    entryCurrent.Highlight(true);
                    entryCurrent.gameObject.SetActive(true);
                    
                    entryNext.Highlight(false);
                    entryNext.gameObject.SetActive(true);
                    
                    entryCurrent.SetTitle((scenarioTraining.trainingSteps[scenarioTraining.currentStepIndex] as GestureTrainingStep).StepTitle.GetLocalizedString());
                    entryNext.SetTitle((scenarioTraining.trainingSteps[scenarioTraining.currentStepIndex+1] as GestureTrainingStep).StepTitle.GetLocalizedString());

                }

            }
        }
    }
}
