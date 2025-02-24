using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StepDisplaySample : MonoBehaviour
{
    [SerializeField] private StepDisplayEntryData stepDisplayEntryData;

    // [SerializeField] private Button leftButton;
    // [SerializeField] private Button rightButton;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    public void ChangeStateOfLastElement()
    {        
        stepDisplayEntryData.StepState = StepEntryState.Active;
    }

}
