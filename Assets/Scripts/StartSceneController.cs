using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DFKI.NMY
{
    public class StartSceneController : MonoBehaviour
    {
        public GreifbARWorldSpaceButton introLevelBtn;
        public GreifbARWorldSpaceButton Level1Btn;
        public GreifbARWorldSpaceButton Level2Btn;
        public GreifbARWorldSpaceButton Level3Btn;

        void Start(){
            
            Assert.IsNotNull(introLevelBtn);
            Assert.IsNotNull(Level1Btn);
            Assert.IsNotNull(Level2Btn);
            Assert.IsNotNull(Level3Btn);

            introLevelBtn.Interactable.selectEntered.AddListener( (args)=>{
                GreifbARApp.instance.StartIntro();
            });
            Level1Btn.Interactable.selectEntered.AddListener( (args)=>{
                GreifbARApp.instance.StartLevel1();
            });
            Level2Btn.Interactable.selectEntered.AddListener( (args)=>{
                GreifbARApp.instance.StartLevel2();
            });
            Level2Btn.Interactable.selectEntered.AddListener( (args)=>{
                GreifbARApp.instance.StartLevel3();
            });

        }

    }
}
