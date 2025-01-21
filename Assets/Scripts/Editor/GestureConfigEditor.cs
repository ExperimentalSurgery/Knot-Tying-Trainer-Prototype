using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DFKI.NMY
{
    
    [CustomEditor(typeof(GestureConfig)),CanEditMultipleObjects]
    public class GestureConfigEditor : Editor
    {
        private GestureConfig config;
        private void OnEnable()
        {
            config = target as GestureConfig;
        }
        
        //Here is the meat of the script
        public override void OnInspectorGUI()
        {
         
            //Convert the weaponSprite (see SO script) to Texture
            Texture2D texture = AssetPreview.GetAssetPreview(config.preview);
            //We crate empty space 80x80 (you may need to tweak it to scale better your sprite
            //This allows us to place the image JUST UNDER our default inspector
            GUILayout.Label("", GUILayout.Height(300), GUILayout.Width(300));
            //Draws the texture where we have defined our Label (empty space)
            GUI.DrawTexture(GUILayoutUtility.GetLastRect(), texture);
            
            //Draw whatever we already have in SO definition
            base.OnInspectorGUI();

        }
    }
}
