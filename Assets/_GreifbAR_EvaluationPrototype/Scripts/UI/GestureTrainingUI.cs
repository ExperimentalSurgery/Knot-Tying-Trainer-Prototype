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

    public void UpdateProgressFill(float normalizedValue)
    {
        if (progressFill && normalizedValue >=0.0f && normalizedValue <=1.0f) {
            progressFill.fillAmount = normalizedValue;
        }
    }


    private void Update()
    {
        // TODO: check if we need to split this for left / right
        if (GestureSequencePlayer.instance.isPlayingLeft) {
            progressFill.fillAmount = GestureSequencePlayer.instance.normalizedProgressLeft;
        }
        if (GestureSequencePlayer.instance.isPlayingRight) {
            progressFill.fillAmount = GestureSequencePlayer.instance.normalizedProgressRight;
        }

    }
}
