using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace DFKI.NMY
{
    public class XRSpeedButtonToggle : MonoBehaviour
    {
        public UnityEvent<bool> OnSpeedToggleChanged = new UnityEvent<bool>();
        [SerializeField] private XRSimpleInteractable buttonInteractable;
        [SerializeField] private Image targetSprite;
        [SerializeField] private Sprite fastSprite;
        [SerializeField] private Sprite slowSprite;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private string fastText = "Normal";
        [SerializeField] private string slowText = "Langsam";
        private bool state = false;

        void Awake() {
            state = false;
            UpdateUI();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A)) {
                OnButtonClicked(null);
            }
        }

        private void OnEnable() {
            buttonInteractable.selectEntered.AddListener(OnButtonClicked);
        }

        private void OnDisable() {
            buttonInteractable.selectEntered.RemoveListener(OnButtonClicked);
        }

        void UpdateUI()
        {
            state = !state;
            targetSprite.sprite = state ? fastSprite : slowSprite;
            title.text = state ? fastText : slowText;
        }

        private void OnButtonClicked(SelectEnterEventArgs args)
        {
            UpdateUI();
            OnSpeedToggleChanged.Invoke(state);
        }
    }
}
