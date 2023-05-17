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
    [SerializeField] private Image progressFill;

    public Scrollbar PlaybackSpeedScrollbar
    {
        get => playbackSpeedScrollbar;
        set => playbackSpeedScrollbar = value;
    }

    public Image ProgressFill
    {
        get => progressFill;
        set => progressFill = value;
    }


    private void Update()
    {
        GestureSequencePlayer player = GestureSequencePlayer.instance;
        // TODO: check if we need to split this for left / right
        if (player.isPlayingLeft){
            progressFill.fillAmount = player.PlayAllSequences ? player.normalizedProgressTotalLeft : player.normalizedProgressLeft;
        }
        if (player.isPlayingRight) {
            progressFill.fillAmount = player.PlayAllSequences ? player.normalizedProgressTotalRight : player.normalizedProgressRight;
        }

    }
}
