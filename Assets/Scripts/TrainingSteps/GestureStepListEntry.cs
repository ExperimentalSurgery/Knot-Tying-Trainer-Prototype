using System;
using TMPro;
using UnityEngine;

[Serializable]
public class GestureStepListEntry : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshPro tmpTitle;

    public void Highlight(bool state) {
        animator.SetBool("highlight",state);
    }

    public void SetTitle(string title)
    {
        tmpTitle.text = title;
    }

}
