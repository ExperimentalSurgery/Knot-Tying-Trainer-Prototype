using UnityEngine;
using UnityEngine.Events;

namespace DFKI.NMY {
    public class KeyTriggerHelper : MonoBehaviour
    {
        public UnityEvent keyPressed;
        [SerializeField] private KeyCode key = KeyCode.U;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(key))
            {
                keyPressed.Invoke();
            }
        }
    }
}
