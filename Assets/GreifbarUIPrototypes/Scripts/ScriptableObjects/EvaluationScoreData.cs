using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EvaluationScoreData", menuName = "GreifbarStuff/EvaluationScoreData", order = 4)]
public class EvaluationScoreData : ScriptableObject
{
    [System.Serializable]
    public struct ScoreData
    {
        public string scoreLetter;
        public float scoreMin;
        public float scoreMax;        
        public string scoreText;
        public Color scoreColor;
    }

    [SerializeField] private List<ScoreData> scoreDataList = new List<ScoreData>();

    public ScoreData GetEvaluationForScore(float score)
    {
        foreach (ScoreData scoreData in scoreDataList)
        {
            if (score >= scoreData.scoreMin && score <= scoreData.scoreMax)
            {
                return scoreData;
            }
        }
        Debug.LogError("No ScoreData found for score " + score);
        return scoreDataList[0];
    }

}
