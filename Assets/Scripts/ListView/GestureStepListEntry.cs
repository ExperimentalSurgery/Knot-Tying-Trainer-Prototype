using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class GestureStepListEntry : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI tmpTitle;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject inactivePlane;
    public void Highlight(bool state) {
        inactivePlane.gameObject.SetActive(!state);
    }

    public void SetTitle(string title) {
        tmpTitle.text = title;
    }

    public void SetIcon(Sprite s) {
        icon.sprite = s;
    }

}
