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
        [SerializeField] private Button skipFrameButton;
        [SerializeField] private Button prevFrameButton;

        private void OnEnable()
        {
            if(playButton)playButton.onClick.AddListener(player.Play);
            if(pauseButton)pauseButton.onClick.AddListener(player.Pause);
            if(resetButton)resetButton.onClick.AddListener(player.Restart);
            if(stopButton)stopButton.onClick.AddListener(player.StopThread);
            if(skipFrameButton)skipFrameButton.onClick.AddListener(player.NextFrame);
            if(prevFrameButton)prevFrameButton.onClick.AddListener(player.PreviousFrame);
        }

        private void OnDisable()
        {
            if(playButton)playButton.onClick.RemoveListener(player.Play);
            if(pauseButton)pauseButton.onClick.RemoveListener(player.Pause);
            if(resetButton)resetButton.onClick.RemoveListener(player.Restart);
            if(stopButton)stopButton.onClick.RemoveListener(player.StopThread);
            if(skipFrameButton)skipFrameButton.onClick.RemoveListener(player.NextFrame);
            if(prevFrameButton)prevFrameButton.onClick.RemoveListener(player.PreviousFrame);
        }   

        private void Update()
        {
            if (player == null) return;
            if(stateTmp)stateTmp.text = player.status.ToString();
            if(frameTmp)frameTmp.text = player.CurrentFrameIndex.ToString() +" of "+player.GetTotalFrames();
        }
    }
}
