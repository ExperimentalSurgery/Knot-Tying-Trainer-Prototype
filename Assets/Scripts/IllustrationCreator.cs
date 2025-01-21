using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using UnityEngine;

namespace DFKI.NMY
{
   
    public class IllustrationCreator : MonoBehaviour
    {
        
        public Texture2D textureToUse;

        [ContextMenu("Create")]
        public void CreateAssets() {
            
            
           List<KnotGestureStep> gesturestep = FindObjectsOfType<KnotGestureStep>(true).ToList();
           
           
            foreach (var s in gesturestep) {
                
                Debug.Log("Create texture for "+s.gameObject.name);
                string path = Application.dataPath + "/Textures/Gestures/";
                string assetName = "Illustration_" + s.gameObject.name.Split("[GestureStep] ")[1] + ".png";
                string assetPath = path + assetName;

                // Encode the texture to PNG format
                byte[] pngBytes = textureToUse.EncodeToPNG();

                // Write the PNG bytes to a file
                System.IO.File.WriteAllBytes(assetPath, pngBytes);
                    
                s.Illustration = LoadSpriteFromPath(assetPath);
                #if UNITY_EDITOR
                EditorUtility.SetDirty(s.gameObject);
                EditorUtility.SetDirty(s);
                
                // Refresh the asset database to make Unity aware of the new asset
                UnityEditor.AssetDatabase.Refresh();
                #endif
            }
        }
        Sprite LoadSpriteFromPath(string path)
        {
#if UNITY_EDITOR
            // Load the sprite using AssetDatabase in the editor
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
#else
        // Load the sprite using Resources.Load at runtime
        return Resources.Load<Sprite>(path);
#endif
        }
    }
}
