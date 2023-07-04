using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DFKI.NMY;
using DFKI.NMY.TrainingSteps;
using Microsoft.MixedReality.Toolkit.Utilities;
using NMY;
using NMY.VirtualRealityTraining;
using NMY.VirtualRealityTraining.Steps;
using NMY.VTT.Core;
using UnityEngine;

public class GestureStepListView : SimpleAnimatorActivatable
{

    [SerializeField] private GridObjectCollection root;
    [SerializeField] private GestureStepListEntry stepRowPrefab;
    [SerializeField] private BaseTrainingStepUnityEvents trainingEvents;


    protected override void StartupEnter()
    {
        base.StartupEnter();
        trainingEvents.stepStartedEvent.AddListener(OnStepStarted);
    }

    

    private void OnStepStarted(BaseTrainingStepEventArgs args)
    {
        Debug.Log("Started Steps args="+args.step.gameObject.name);
        // Cleanup UI
        foreach (Transform child in root.transform) {
            GameObject.Destroy(child.gameObject);
        }

        var candidatesAsChilds = args.step.transform.GetComponentsInChildren<BaseTrainingStep>();
        Debug.Log("found child steps c="+candidatesAsChilds.Length);
        foreach (var childStep in candidatesAsChilds)
        {
            
                GestureStepListEntry entry = Instantiate(stepRowPrefab.gameObject, root.transform).GetComponent<GestureStepListEntry>();
                entry.gameObject.name = "trainingstep";
                entry.connectedStep = childStep as KnotGestureBaseStep;
            
        }
        root.UpdateCollection();

    }


}
