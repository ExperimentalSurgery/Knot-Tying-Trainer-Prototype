using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GestureStepListEntry : MonoBehaviour
{

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
    
}
