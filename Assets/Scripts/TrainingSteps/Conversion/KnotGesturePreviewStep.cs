using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnotGesturePreviewStep : KnotGestureBaseStep
{
    
    [Header("Sequences Config")]
    [SerializeField] private float singleSequenceDuration = 2f;
        
    // runtime vars
    private bool finishedLeft = false;
    private bool finishedRight = false;

}
