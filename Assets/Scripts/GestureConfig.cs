using System;
using System.Collections.Generic;
using UnityEngine;

namespace DFKI.NMY
{
    
    
    [CreateAssetMenu(fileName = "GestureConfig_", menuName = "KnotbAR/Gestures", order = 1)]
    [Serializable]
    public class GestureConfig : ScriptableObject
    {
        [Header("GestureConfig")]
        [SerializeField] public string gestureKey = "empty";

        [Header("BVH")]
        
        [SerializeField] public string bvhPathLeft="empty";
        [SerializeField] public string bvhPathRight="empty";
        [SerializeField] public int sequenceLeft = 0;
        [SerializeField] public int sequenceRight = 0;
        [Header("PointCloud")]
        [SerializeField] public string pointCloudFilePath = "empty";
        [SerializeField] public bool manipulateFPS = true;
        [SerializeField] public int targetFPS = 15;
        
        [Header("BVH Thresholds")] 
        [SerializeField] [Range(0,15)] public float poseMatchingThresholdLeft = 5;
        [SerializeField] [Range(0,15)] public float poseMatchingThresholdRight = 5;
        
        [Header("BVH Finger Configuration - Left")]
        [SerializeField] [Range(0f,1f)] public float thumbLeftWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] public float indexLeftWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] public float middleLeftWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] public float ringLeftWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] public float pinkyLeftWeight = 1.0f;
        
        [Header("BVH Finger Configuration - Right")]
        [SerializeField] [Range(0f,1f)] public float thumbRightWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] public float indexRightWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] public float middleRightWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] public float ringRightWeight = 1.0f;
        [SerializeField] [Range(0f,1f)] public float pinkyRightWeight = 1.0f;
        
        [Header("Wrist Rotation Weights - Left")]
        [SerializeField] [Range(0f,15f)] public float wristXLeftWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] public float wristYLeftWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] public float wristZLeftWeight = 5.0f;
        
        [Header("Wrist Rotation Weights - Right")]
        [SerializeField] [Range(0f,15f)] public float wristXRightWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] public float wristYRightWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] public float wristZRightWeight = 5.0f;
        
        [Header("Elbow Rotation Weights - Left")]
        [SerializeField] [Range(0f,15f)] public float elbowXLeftWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] public float elbowYLeftWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] public float elbowZLeftWeight = 5.0f;
        
        [Header("Elbow Rotation Weights - Right")]
        [SerializeField] [Range(0f,15f)] public float elbowXRightWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] public float elbowYRightWeight = 0.5f;
        [SerializeField] [Range(0f,15f)] public float elbowZRightWeight = 5.0f;

        
        [Header("Illustration")]
        [SerializeField] public Sprite preview;

    }
}
