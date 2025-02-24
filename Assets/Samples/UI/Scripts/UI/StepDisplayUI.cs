using System.Collections;
using System.Collections.Generic;
using DFKI.NMY;
using NMY.VirtualRealityTraining;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;

public class StepDisplayUI : MonoBehaviour
{    
    [SerializeField] private StepDisplayData stepDisplayData;

    [SerializeField] private GameObject entryContainer;

    [SerializeField] private List<BaseTrainingStep> connectedSteps;

    private Dictionary<BaseTrainingStep, StepEntryUI> runtimeMapping = new Dictionary<BaseTrainingStep, StepEntryUI>();
    
    public StepDisplayData StepDisplayData
    {
        get => stepDisplayData;
        set {
            stepDisplayData = value;
            UpdateUI();
        }
    }

    [SerializeField] private GameObject stepEntryPrefab;
    
    void OnEnable()
    {        
        if (stepDisplayData)
            UpdateUI();        
    }
    
    public void UpdateUI()
    {
        if (stepDisplayData == null) return;

        runtimeMapping.Clear();
        // remove all children from container
        foreach (Transform child in entryContainer.transform)
        {
            Destroy(child.gameObject);
        }

        int i = 1;
        foreach (BaseTrainingStep step in connectedSteps) {
            
            StepDisplayEntryData data = ScriptableObject.CreateInstance<StepDisplayEntryData>();
            data.StepTitle = (step is GreifbarChapter)? (step as GreifbarChapter).ChapterTitle.GetLocalizedString(): gameObject.name;
            data.ConnectedStep = step;
            data.StepIndex = i;
            switch (step.stepState)
            {
                case BaseTrainingStep.StepState.StepCompleted:
                    data.StepState = StepEntryState.Completed;
                    break;
                case BaseTrainingStep.StepState.StepStarted:
                    data.StepState = StepEntryState.Active;
                    break;
                case BaseTrainingStep.StepState.StepFinished:
                    data.StepState = StepEntryState.Active;
                    break;
                case BaseTrainingStep.StepState.StepWaiting:
                    data.StepState = StepEntryState.Todo;
                    break;
                default:
                    data.StepState = StepEntryState.Todo;
                    break;
            }
            
            
            var go = Instantiate(stepEntryPrefab, entryContainer.transform);
            var stepEntryUI = go.GetComponent<StepEntryUI>();

            stepEntryUI.StepDisplayEntryData = data;
            runtimeMapping.Add(step,stepEntryUI);
            step.OnStepStarted -= OnStepStarted;
            step.OnStepStarted += OnStepStarted;
            step.OnStepCompleted -= OnStepCompleted;
            step.OnStepCompleted += OnStepCompleted;
            
            i++;
        }

        return;
        foreach (var stepEntryData in stepDisplayData.StepEntries)
        {
            var go = Instantiate(stepEntryPrefab, entryContainer.transform);
            var stepEntryUI = go.GetComponent<StepEntryUI>();

            stepEntryUI.StepDisplayEntryData = stepEntryData;
            // stepEntryUI.UpdateUI();
        }
    }

    private void OnStepCompleted(object sender, BaseTrainingStepEventArgs e)
    {
        if (runtimeMapping.ContainsKey(e.step))
        {
            if (runtimeMapping[e.step].StepDisplayEntryData.StepState != StepEntryState.Completed)
            {
                runtimeMapping[e.step].StepDisplayEntryData.StepState = StepEntryState.Completed;
                runtimeMapping[e.step].StepDisplayEntryData.StateChanged.Invoke();
            }
        }
    }

    private void OnStepStarted(object sender, BaseTrainingStepEventArgs e)
    {
        if (runtimeMapping.ContainsKey(e.step))
        {
            if (runtimeMapping[e.step].StepDisplayEntryData.StepState != StepEntryState.Active)
            {
                runtimeMapping[e.step].StepDisplayEntryData.StepState = StepEntryState.Active;
                runtimeMapping[e.step].StepDisplayEntryData.StateChanged.Invoke();
            }
        }
    }
}
