using System;
using System.Collections;
using System.Collections.Generic;
using DFKI.NMY;
using DFKI.NMY.TrainingSteps;
using NMY.VirtualRealityTraining.Steps;
using TMPro;
using UnityEngine;

public class GestureStepListEntry : MonoBehaviour
{

    public KnotGestureBaseStep connectedStep;
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshPro tmpTitle;

    public void Highlight(bool state)
    {
        animator.SetBool("show",state);
    }

    public void SetTitle(string title)
    {
        tmpTitle.text = title;
    }


    private void Update()
    {
        if (connectedStep) {
            tmpTitle.text = "Schritt";
            animator.SetBool("highlight",connectedStep.stepState.Equals(BaseTrainingStep.StepState.StepStarted));
        }
    }
}
