using System;
using System.Collections;
using System.Collections.Generic;
using DFKI.NMY;
using DFKI.NMY.TrainingSteps;
using Microsoft.MixedReality.Toolkit.Utilities;
using NMY;
using NMY.VTT.Core;
using UnityEngine;

public class GestureStepListView : SimpleAnimatorActivatable
{

    [SerializeField] private GridObjectCollection root;
    [SerializeField] private GestureTrainingController training;
    [SerializeField] private GestureStepListEntry stepRowPrefab;
    
    private GestureScenario currenTraining;
    
    private void Awake()
    {
                
        //entryPrevious.gameObject.SetActive(false);
        //entryCurrent.gameObject.SetActive(false);
        //entryNext.gameObject.SetActive(false);
        
        training.EntryActivationEvent -= OnTrainingStarted;
        training.EntryActivationEvent += OnTrainingStarted;

        foreach (var t in training.trainings)
        {
            t.EntryCompletedEvent -= OnStepCompleted;
            t.EntryCompletedEvent += OnStepCompleted;
        }
        
    }

    private void OnStepCompleted(object sender, ListControllerEventArgs e) {
     
    }

    private void OnTrainingStarted(object sender, ListControllerEventArgs e)
    {
        
        currenTraining = e.caller as GestureScenario;
        
        foreach (Transform child in root.transform) {
            GameObject.Destroy(child.gameObject);
        }
        foreach (var step in currenTraining.trainingSteps)
        { 
            
            GestureStepListEntry entry = Instantiate(stepRowPrefab.gameObject, root.transform).GetComponent<GestureStepListEntry>();
            entry.gameObject.name = "trainingstep";
            entry.connectedStep = step as GestureBaseStep;
        }

        root.UpdateCollection();

    }

    private void Update()
    {
    }
}
