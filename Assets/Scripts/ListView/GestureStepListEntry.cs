using System;
using TMPro;
using UnityEngine;

[Serializable]
public class GestureStepListEntry : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshPro tmpTitle;
    [SerializeField] private SpriteRenderer sprite;
    public void Highlight(bool state) {
        animator.SetBool("highlight",state);
    }

    public void SetTitle(string title) {
        tmpTitle.text = title;
    }

    public void SetIcon(Sprite s) {
        sprite.sprite = s;
    }

}
