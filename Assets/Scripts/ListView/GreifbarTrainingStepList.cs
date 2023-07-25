using System;
using Microsoft.MixedReality.Toolkit.Utilities;
using NMY.VirtualRealityTraining;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;

namespace DFKI.NMY
{
    public class GreifbarTrainingStepList : TrainingStepBaseList<GreifbarTaskItem>
    {
        [SerializeField] private GestureStepListEntry rowEntryPrefab;
        [SerializeField] private GridObjectCollection grid;
        public bool chapterActive;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            foreach (Transform child in grid.transform) {
                Destroy(child.gameObject);
            }
            
            foreach (var item in _taskList)
            {
                GestureStepListEntry spawned = Instantiate(rowEntryPrefab,grid.transform);
                GreifbarTaskItem converted = item as GreifbarTaskItem;
                converted.SetListItem(spawned);
                converted.onTaskStarted.AddListener(OnTaskHasStarted);
                converted.onTaskCompleted.AddListener(OnTaskHasCompleted);
                converted.SetTitle(converted.task.gameObject.name);
                converted.Highlight(item.task.stepState.Equals(BaseTrainingStep.StepState.StepStarted));

                if (converted.task as GreifbarBaseStep) {
                    converted.SetTitle((converted.task as GreifbarBaseStep).StepTitle.GetLocalizedString());
                }
                else if(converted.task as GreifbarVirtualAssistantStep){
                    converted.SetTitle((converted.task as GreifbarVirtualAssistantStep).StepTitle.GetLocalizedString());
                }
                else
                {
                    converted.SetTitle(converted.task.gameObject.name);
                }
                
            }
            grid.UpdateCollection();
        }

        private void Update()
        {
            foreach (var item in _taskList) {
                GreifbarTaskItem converted = item as GreifbarTaskItem;
                converted.Highlight(item.task.stepState.Equals(BaseTrainingStep.StepState.StepStarted));
            }
            
        }

        private void OnTaskHasCompleted(BaseTaskItem item) {
            // Prepared
        }
        private void OnTaskHasStarted(BaseTaskItem item) {
            //Prepared
        }

        public void SetChapterState(bool val)
        {
            chapterActive = val;
        }
    }
    
    [Serializable]
    public class GreifbarTaskItem : BaseTaskItem
    {
        [HideInInspector][SerializeField] private GestureStepListEntry listItem;

        public void SetListItem(GestureStepListEntry entry) => listItem = entry;
        public void SetTitle(string name)=>listItem.SetTitle(name);
        public void Highlight(bool state) => listItem.Highlight(state);
         
    }
    
}
