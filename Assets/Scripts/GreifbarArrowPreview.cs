using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;
using UnityEngine.Assertions;

namespace DFKI.NMY
{
    public class GreifbarArrowPreview : MonoBehaviour
    {
        

        public string gestureKey="empty";
        public AlembicStreamPlayer alembicStreamPlayer;
        public float currentTime = 0f;


        
        
        // Start is called before the first frame update
        void Start() {
            Assert.IsNotNull(alembicStreamPlayer);
            Assert.IsFalse(gestureKey=="empty","Key empty for "+this.gameObject.name);
        }


        public void SetNormalizedTime(float lerp)
        {
            if (lerp < 0 || lerp >= 1) return;
            currentTime = alembicStreamPlayer.Duration * lerp;
            alembicStreamPlayer.CurrentTime = currentTime;
        }

        public void SetTime(float time)
        {
            currentTime = time;
            alembicStreamPlayer.CurrentTime = currentTime;
        }

        void Reset() {
            alembicStreamPlayer = GetComponent<AlembicStreamPlayer>();
            currentTime = 0f;
            alembicStreamPlayer.CurrentTime = 0f;
        }
    }
}
