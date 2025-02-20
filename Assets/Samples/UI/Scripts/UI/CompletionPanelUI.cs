using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DFKI.NMY;

public class CompletionPanelUI : MonoBehaviour
{
    [SerializeField] private CompletionPanelData completionPanelData;
    public CompletionPanelData CompletionPanelData
    {
        get { return completionPanelData; }
        set { 
            completionPanelData = value; 
            UpdateUI();
        }
    }

    [Tooltip("ScriptableObject that contains the data for the evaluation score.")]
    [SerializeField] private EvaluationScoreData evaluationScoreData;

    [Header("UI Elements")] 
    [SerializeField] private GameObject rootTechnique;
    [SerializeField] private GameObject rootTime;
    [SerializeField] private GameObject rootTension;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text knotTechnqiueValueText;
    [SerializeField] private TMP_Text tensionValueText;
    [SerializeField] private TMP_Text totalTimeValueText;

    [SerializeField] private TMP_Text totalValueLetter;
    [SerializeField] private TMP_Text totalValueText;
    [SerializeField] private Image totalValueImage;

    // Start is called before the first frame update
    void Start()
    {
        if (completionPanelData != null) 
            UpdateUI();        
    }

    private void Update()
    {
        UpdateUI();
    }
    
    public string ConvertSecondsToMMSS(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public void UpdateUI()
    {

        switch (completionPanelData.Phase)
        {
            case TrainingPhase.L1_KNOTENTECHNIK:
                rootTechnique.gameObject.SetActive(true);
                rootTension.gameObject.SetActive(false);
                rootTime.gameObject.SetActive(false);
                break;
            case TrainingPhase.L2_KNOTENSPANNUNG:
                rootTechnique.gameObject.SetActive(true);
                rootTension.gameObject.SetActive(true);
                rootTime.gameObject.SetActive(false);
                break;
            case TrainingPhase.L3_ZEITMESSUNG:
                rootTechnique.gameObject.SetActive(true);
                rootTension.gameObject.SetActive(true);
                rootTime.gameObject.SetActive(true);
                break;
        }
        
        //titleText.text = $"Abschluss: {completionPanelData.Title}";
        knotTechnqiueValueText.text = $"{completionPanelData.KnotTechniqueValue} Fehler";
        tensionValueText.text = $"{completionPanelData.TensionValue}%";
        //int minutes = (int)(completionPanelData.TotalTimeSecValue / 60f);
        //int seconds = (int)(completionPanelData.TotalTimeSecValue % 60f);
        //totalTimeValueText.text = completionPanelData.TotalTimeSecValue.ToString($"{minutes}:{seconds:D2}");
        totalTimeValueText.text = ConvertSecondsToMMSS(completionPanelData.TotalTimeSecValue);
        totalValueImage.fillAmount = ((int)completionPanelData.TensionValue)/100f;
        
        totalValueImage.color = evaluationScoreData.GetEvaluationForScore(completionPanelData.TensionValue).scoreColor;
        totalValueText.text = evaluationScoreData.GetEvaluationForScore(completionPanelData.TensionValue).scoreText;
        totalValueLetter.text = evaluationScoreData.GetEvaluationForScore(completionPanelData.TensionValue).scoreLetter;
    }
}
