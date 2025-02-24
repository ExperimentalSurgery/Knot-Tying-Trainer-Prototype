using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace DFKI.NMY
{
    public class NmyXRHoverFillEffect : MonoBehaviour
    {
        [Header("NMY")] 
        [SerializeField] private XRBaseInteractable _interactable;
        [SerializeField] private Image buttonSelectFill;
        
       
        [Header("Selection Fill")] 
        private float fillDuration;
        private float remainingDuration;
        
        private void OnEnable() {
            fillDuration = _interactable.gazeTimeToSelect;
            _interactable.firstHoverEntered.AddListener(OnHoverEntered);
        }

        protected virtual void OnHoverEntered(HoverEnterEventArgs args) {
            if(args.interactorObject as XRRayInteractor) {
                remainingDuration = fillDuration;
            }
            else {
                remainingDuration = 0;
            }
        }

        private void Update() {
            if (_interactable.isSelected) {
                buttonSelectFill.fillAmount = 1.0f;
            }
            else {
                remainingDuration += _interactable.isHovered ? -Time.deltaTime : Time.deltaTime * 2;
                var lerp = Mathf.Clamp01(remainingDuration / fillDuration);
                buttonSelectFill.fillAmount = 1.0f - lerp;
            }
        }

    }
}
