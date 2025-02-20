using System.Collections;
using System.Collections.Generic;
using DFKI.NMY;
using UnityEngine;

[CreateAssetMenu(fileName = "CompletionPanelData", menuName = "GreifbarStuff/CompletionPanelData", order = 3)]
public class CompletionPanelData : ScriptableObject
{
    [SerializeField] private TrainingPhase phase;

    public TrainingPhase Phase
    {
        get => phase;
        set => phase = value;
    }

    [SerializeField] private string title;
    public string Title
    {
        get { return title; }
        set { title = value; }
    }

    [SerializeField] private int knotTechniqueValue;
    public int KnotTechniqueValue
    {
        get { return knotTechniqueValue; }
        set { knotTechniqueValue = value; }
    }

    [SerializeField] private float tensionValue;
    public float TensionValue
    {
        get { return tensionValue; }
        set { tensionValue = value; }
    }

    [SerializeField] private float totalTimeSecValue;
    public float TotalTimeSecValue
    {
        get { return totalTimeSecValue; }
        set { totalTimeSecValue = value; }
    }
}
