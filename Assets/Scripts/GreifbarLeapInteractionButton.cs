using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;
using NMY;

namespace DFKI.NMY
{
    public class GreifbarLeapInteractionButton : InteractionButton
    {
        [Header("Greifbar 3D Button")]
        [SerializeField] public ActivatableStartupBehaviour highlighter;

        public void ShowHiglighter(){
            if(highlighter)highlighter.Activate();
        }

        public void HideHighlighter(){
            if(highlighter)highlighter.Deactivate();
        }
    }
}
