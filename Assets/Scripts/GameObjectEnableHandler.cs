using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DFKI.NMY
{
    public class GameObjectEnableHandler : MonoBehaviour
    {

        [SerializeField] private List<GameObject> enable;
        [SerializeField] private List<GameObject> disable;

        private void OnEnable()
        {
            foreach (var g in enable)
            {
                g.SetActive(true);
            }
            
            foreach (var g in disable)
            {
                g.SetActive(false);
            }
        }

       
    }
}
