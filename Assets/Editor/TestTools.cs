using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DFKI.NMY
{
    public static class TestTools {
        // Start is called before the first frame update
        
        static public List<T> GetAssetsOfType<T>() where T : class
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            foreach (string guid in guids) {
                assets.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(T)) as T);
            }
            return assets;
        }
    }
}
