using System.Collections;
using System.Collections.Generic;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "StepDisplayEntryData", menuName = "GreifbarStuff/StepDisplayEntryData", order = 2)]
public class StepDisplayEntryData : ScriptableObject
{
    [SerializeField] private int stepIndex;
    [SerializeField] private string stepTitle;

    [SerializeField] private StepEntryState stepState;
    [SerializeField] private BaseTrainingStep connectedStep;

    public BaseTrainingStep ConnectedStep
    {
        get => connectedStep;
        set => connectedStep = value;
    }

    public StepEntryState StepState
    {
        get => stepState;
        set {
            stepState = value;
            StateChanged.Invoke();
        }
    }

    public int StepIndex
    {
        get => stepIndex;
        set => stepIndex = value;
    }

    public string StepTitle
    {
        get => stepTitle;
        set => stepTitle = value;
    }

  
    public UnityEvent StateChanged = new UnityEvent();

}
