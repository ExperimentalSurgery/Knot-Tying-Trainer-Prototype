using System.Collections.Generic;
using UnityEngine;

namespace DFKI.NMY
{
    public class MeshGesturePreview : MonoBehaviour
    {

        [SerializeField] public string gestureKey = "empty";
        
        [Header("Config")] 
        
        [SerializeField][HideInInspector] public float fadeInDuration = 0.5f;
        [SerializeField][HideInInspector] public float fadeOutDuration = 0.5f;
        
        [Header("Hand")] 
        [SerializeField] private MeshRenderer handLMeshRenderer;
        [SerializeField] private MeshRenderer handRMeshRenderer;
        
        [Header("Faden")]
        [SerializeField] private MeshRenderer fadenLMeshRenderer;
        [SerializeField] private MeshRenderer fadenRMeshRenderer;
        [Header("(Optional) Additional Renderers")]
        [SerializeField] private List<MeshRenderer> additionalRenderers;
        
        private float fadeRemainingDuration;
        private bool fadeIn = false;
        private bool fadeOut = false;

        
        void Reset()
        {

            var renderers = GetComponentsInChildren<MeshRenderer>(true);

            foreach (var r in renderers)
            {
                if (r.gameObject.name.Contains("Hand"))
                {
                    if (r.gameObject.name.Contains("L"))
                    {
                        handLMeshRenderer = r;
                    }
                    else if (r.gameObject.name.Contains("R"))
                    {
                        handRMeshRenderer = r;
                    }
                }
                
                else if (r.gameObject.name.Contains("Faden") || r.gameObject.name.Contains("faden"))
                {
                    if (r.gameObject.name.Contains("L"))
                    {
                        fadenLMeshRenderer = r;
                    }
                    else if (r.gameObject.name.Contains("R"))
                    {
                        fadenRMeshRenderer = r;
                    }
                }

                this.gestureKey = gameObject.name;
            }

        }
        
        private void OnEnable()
        {
            SetMeshAlpha(0.0f); 
        }

        private void Update()
        {
            if (fadeOut || fadeIn)
            {
                fadeRemainingDuration -= Time.deltaTime;
                float fadeDuration = 0.0f;
                if (fadeIn)
                {
                    fadeDuration = fadeInDuration;
                }
                if (fadeOut)
                {
                    fadeDuration = fadeOutDuration;
                }
                
                float lerpVal = 1f-(fadeRemainingDuration / fadeDuration);
                if (lerpVal >= 1.0)
                {
                    fadeRemainingDuration=0.0f;
                    lerpVal = 1f;
                    
                    if (fadeIn) {
                        SetMeshAlpha(Mathf.Lerp(0f, 1f, lerpVal));
                        fadeIn = false;
                        
                    }
                    if (fadeOut) {
                        SetMeshAlpha(Mathf.Lerp(1f, 0f, lerpVal));
                        fadeOut = false;
                    }
                }
                if (fadeIn) {
                    SetMeshAlpha(Mathf.Lerp(0f, 1f, lerpVal));
                }
                if (fadeOut) {
                    SetMeshAlpha(Mathf.Lerp(1f, 0f, lerpVal));
                }
                 
                
            }
        }

        void SetMeshAlpha(float nextAlpha)
        {
            if(fadenLMeshRenderer) fadenLMeshRenderer.material.SetFloat("_Alpha",nextAlpha);
            if(fadenLMeshRenderer) fadenRMeshRenderer.material.SetFloat("_Alpha",nextAlpha);
            if(handLMeshRenderer) handLMeshRenderer.material.SetFloat("_Alpha",nextAlpha);
            if(handRMeshRenderer) handRMeshRenderer.material.SetFloat("_Alpha",nextAlpha);

            foreach (MeshRenderer ren in additionalRenderers) {
                foreach (Material mat in ren.materials) {
                    mat.SetFloat("_Alpha", nextAlpha);
                }
            }
        }
        
        [ContextMenu("Fade In")]
        public void FadeIn()
        {
            if (fadeIn) return;
            
            fadeIn = true;
            fadeOut = false;
            fadeRemainingDuration = fadeOutDuration;
        }
        
        [ContextMenu("Fade Out")]
        public void FadeOut()
        {
            if (fadeOut) return;
            fadeOut = true;
            fadeIn = false;
            fadeRemainingDuration = fadeOutDuration;
        }
        
    }
}
