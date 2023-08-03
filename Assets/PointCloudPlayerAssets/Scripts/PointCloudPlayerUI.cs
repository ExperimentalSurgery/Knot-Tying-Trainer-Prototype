using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DFKI.NMY.PoincloudPlayer
{
    public class PointCloudPlayerUI : MonoBehaviour{   
        
        [SerializeField] private PointCloudPlayer player;
        [SerializeField] private TextMeshProUGUI stateTmp;
        [SerializeField] private TextMeshProUGUI frameTmp;
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button stopButton;

        private void OnEnable()
        {
            if(playButton)playButton.onClick.AddListener(player.Play);
            if(pauseButton)pauseButton.onClick.AddListener(player.Pause);
            if(resetButton)resetButton.onClick.AddListener(player.Restart);
            if(stopButton)stopButton.onClick.AddListener(player.StopThread);
        }

        private void OnDisable()
        {
            if(playButton)playButton.onClick.RemoveListener(player.Play);
            if(pauseButton)pauseButton.onClick.RemoveListener(player.Pause);
            if(resetButton)resetButton.onClick.RemoveListener(player.Restart);
            if(stopButton)stopButton.onClick.RemoveListener(player.StopThread);
        }   

        private void Update()
        {
            if (player == null) return;
            if(stateTmp)stateTmp.text = player.status.ToString();
            if(frameTmp)frameTmp.text = player.CurrentFrameIndex.ToString();
        }
    }
}
