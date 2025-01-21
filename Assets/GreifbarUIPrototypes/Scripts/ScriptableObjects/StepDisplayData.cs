using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StepDisplayData", menuName = "GreifbarStuff/StepDisplayData", order = 1)]
public class StepDisplayData : ScriptableObject
{
    [SerializeField] private List<StepDisplayEntryData> stepEntries;
    public IEnumerable<StepDisplayEntryData> StepEntries => stepEntries;
    public int StepEntriesCount => stepEntries.Count;    
    
}
