using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace DFKI.NMY
{
    public class MeshGesturePreviewManager : MonoBehaviour
    {

        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.25f;
        
        public MeshGesturePreview currentPreview { private set; get; }
        public GreifbarArrowPreview currentArrowPreview { private set; get; }
        
        private List<MeshGesturePreview> previews = new List<MeshGesturePreview>();
        private List<GreifbarArrowPreview> arrows = new List<GreifbarArrowPreview>();
        [SerializeField]
        private TextMeshProUGUI currentKeyDebug;

        [SerializeField] private GameObject previewRoot;
        [SerializeField] private GameObject arrowRoot;
        
        

        public bool ArrowActive() => currentArrowPreview != null;
        public bool PreviewActive() => currentPreview != null;
        
        private void Awake()
        {
            FillPreviews();
        }

        public void FillPreviews()
        {
            previews = previewRoot.GetComponentsInChildren<MeshGesturePreview>(true).ToList();
            arrows = arrowRoot.GetComponentsInChildren<GreifbarArrowPreview>(true).ToList();
        }

        public void HideGesturePreview()
        {
            foreach (MeshGesturePreview gesture in previews) {
                gesture.gameObject.SetActive(false);
            }
            currentPreview = null;
        }

        public void UpdateCurrentGesture(string gestureKey) {

            if (currentKeyDebug) {
                currentKeyDebug.text = gestureKey;
            }

            currentPreview = null;
            currentArrowPreview = null;
            // Search Gesture
            foreach (MeshGesturePreview gesture in previews) {
                if (gesture.gestureKey.Equals(gestureKey)) {
                    gesture.gameObject.SetActive(true);
                    currentPreview = gesture;
                }
                else {
                    gesture.gameObject.SetActive(false);
                }
            }
            
           UpdateArrowPreview(gestureKey);
            
        }

        public void UpdateArrowPreview(string gestureKey)
        {
            // Search Arrows
            foreach (GreifbarArrowPreview p in arrows) {

                if (p.gestureKey == gestureKey) {
                    p.gameObject.SetActive(true);
                    currentArrowPreview = p;
                }
                else
                {
                    p.gameObject.SetActive(false);
                }
            }
        }

        public bool ArrowPreviewAvailable(string gestureKey)
        {
            FillPreviews();
            foreach (GreifbarArrowPreview gesture in arrows) {
                if (gesture.gestureKey.Equals(gestureKey))
                {
                    return true;
                }
            }
            return false;
        }

        public bool PreviewAvailable(string gestureKey)
        {
            FillPreviews();
            foreach (MeshGesturePreview gesture in previews) {
                if (gesture.gestureKey.Equals(gestureKey))
                {
                    return true;
                }
            }
            return false;
        }

        public GreifbarArrowPreview GetActiveArrowPreview()
        {
            if (currentArrowPreview)
            {
                return currentArrowPreview;
            }

            return null;
        }
        
        public void ShowArrowPreview()
        {
            if (currentArrowPreview)
            {
                currentArrowPreview.gameObject.SetActive(true);
            }
        }
        
        public void HideArrowPreview()
        {
            if (currentArrowPreview)
            {
                currentArrowPreview.gameObject.SetActive(false);
            }

            currentArrowPreview = null;
        }


        
        public void FadeInCurrent()
        {
            if (currentPreview == null) return;
            if (PreviewActive() == false) return;
            
            currentPreview.fadeInDuration = fadeInDuration;
            currentPreview.fadeOutDuration = fadeOutDuration;
            currentPreview.FadeIn();
        }

        public void FadeOutCurrent() {
            if (currentPreview == null) return;
            if (PreviewActive() == false) return;
            
            currentPreview.fadeInDuration = fadeInDuration;
            currentPreview.fadeOutDuration = fadeOutDuration;
            currentPreview.FadeOut();
        }

    }
}
