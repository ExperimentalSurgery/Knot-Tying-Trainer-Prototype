using NMY.VirtualRealityTraining.Steps.VirtualAssistant;
using UnityEngine;
using UnityEngine.Localization;

namespace DFKI.NMY
{
    public class GreifbarVirtualAssistantStep : VirtualAssistantSpeakStep {
    
         [Header("Localized Step Data")] 
         [SerializeField] private LocalizedString stepTitle;
         [SerializeField] private LocalizedString stepDescription;
    
         public LocalizedString StepTitle{
            get => stepTitle;
            set => stepTitle = value;
            }

        public LocalizedString StepDescription{
            get => stepDescription;
            set => stepDescription = value;
        }
    }
}
