using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using NMY;
using UnityEngine;

namespace DFKI.NMY
{
    public class GreifbarMRTKInteractable : Interactable
    {
        [SerializeField] public ActivatableStartupBehaviour highlighter;

        public void ShowHiglighter()
        {
            highlighter.Activate();
        }

        public void HideHighlighter()
        {
            highlighter.Deactivate();
        }
        
    }
}
