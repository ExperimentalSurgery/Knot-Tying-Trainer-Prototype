using System;
using System.Collections;
using System.Collections.Generic;
using DFKI.NMY;
using NMY;
using UnityEngine;
using UnityEngine.UI;

public class GestureTrainingUI : SingletonStartupBehaviour<GestureTrainingUI>
{

    [SerializeField] private Scrollbar playbackSpeedScrollbar;
    [SerializeField] private Image progressFillLeft;
    [SerializeField] private Image progressFillRight;
    
    public Scrollbar PlaybackSpeedScrollbar
    {
        get => playbackSpeedScrollbar;
        set => playbackSpeedScrollbar = value;
    }

    private void Update()
    {
        GestureSequencePlayer player = GestureSequencePlayer.instance;
        if (player.isPlayingLeft){
            progressFillLeft.fillAmount = player.PlayAllSequences ? player.normalizedProgressTotalLeft : player.normalizedProgressLeft;
        }
        if (player.isPlayingRight) {
            progressFillRight.fillAmount = player.PlayAllSequences ? player.normalizedProgressTotalRight : player.normalizedProgressRight;
        }

    }
}
