using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NMY.VirtualRealityTraining;

namespace DFKI.NMY
{
    public class NextStepTrigger : MonoBehaviour
    {
        public KeyCode triggerKey;
        private TrainingStepController stepController;

        // Start is called before the first frame update
        void Start()
        {
            stepController = FindObjectOfType<TrainingStepController>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(triggerKey))
            {
                stepController.currentStep.ForceContinue();
            }
        }
    }
}