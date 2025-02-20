using System.Collections;
using System.Collections.Generic;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StepEntryUI : MonoBehaviour
{
    [SerializeField] private StepDisplayEntryData stepDisplayEntryData;
    public StepDisplayEntryData StepDisplayEntryData
    {
        get => stepDisplayEntryData;
        set {
            if (stepDisplayEntryData)
                DisconnectListeners();
            stepDisplayEntryData = value;
            ConnectListeners();
            UpdateUI();
        }
    }

    [Header("UI Elements")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text stepIndexText;
    [SerializeField] private TMP_Text stepTitleText;

    void Start()
    {
        if (stepDisplayEntryData)
            ConnectListeners();

        UpdateUI();
    }

    void ConnectListeners()
    {     
        stepDisplayEntryData.StateChanged.AddListener(UpdateUI);
    }

    void DisconnectListeners()
    {        
        stepDisplayEntryData.StateChanged.RemoveListener(UpdateUI);
    }

    public Color activeColor = Color.green;
    public Color pendingColor = Color.white;
    public Color completedColor = Color.gray;
    public Color textColorCompleted = Color.gray;
    public Color textColorDefault = Color.gray;
    
    public void UpdateUI()
    {
        //Debug.Log("UpdateUI: " + stepDisplayEntryData.StepIndex.ToString("00")+" state="+stepDisplayEntryData.StepState.ToString());

        if (stepDisplayEntryData == null) return;

        stepIndexText.text = stepDisplayEntryData.StepIndex.ToString("0");
        stepTitleText.text = stepDisplayEntryData.StepTitle;

        //Color c = backgroundImage.color;
        if (stepDisplayEntryData.StepState == StepEntryState.Completed)
        {         
            //c = Color.gray;   
            //c.a = 0.5f;
            backgroundImage.color = completedColor;
            stepIndexText.color = textColorCompleted;
            stepTitleText.color = textColorCompleted;
        }
        else if (stepDisplayEntryData.StepState == StepEntryState.Active)
        {
            //c.a = 1f;
            backgroundImage.color = activeColor;
            stepIndexText.color = textColorDefault;
            stepTitleText.color = textColorDefault;
        }
        else
        {
            //c = Color.gray;
            //c.a = 0.75f;
            backgroundImage.color = pendingColor;
            stepIndexText.color = textColorDefault;
            stepTitleText.color = textColorDefault;
        }
    }
}
