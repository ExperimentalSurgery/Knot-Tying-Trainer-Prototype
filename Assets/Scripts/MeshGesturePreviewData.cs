using UnityEngine;

namespace DFKI.NMY {
    
    [CreateAssetMenu(fileName = "MeshGesturePreview_", menuName = "GreifbarStuff/Gestures/MeshGesturePreview/", order = 1)]
    public class MeshGesturePreviewData : ScriptableObject {
        
        public Mesh mesh;
        public Texture texture;
        
    }
}
