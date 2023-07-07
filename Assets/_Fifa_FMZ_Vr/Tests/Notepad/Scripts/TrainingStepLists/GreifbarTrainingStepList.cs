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
        protected override void OnEnable()
        {
            base.OnEnable();
            
            foreach (Transform child in grid.transform) {
                Destroy(child.gameObject);
            }
            
            foreach (var item in _taskList)
            {
                GestureStepListEntry spawned = Instantiate(rowEntryPrefab,grid.transform);
                spawned.gameObject.name = item.task.gameObject.name;
                GreifbarTaskItem converted = item as GreifbarTaskItem;
                converted.SetListItem(spawned);
                converted.onTaskStarted.AddListener(OnTaskHasStarted);
                converted.onTaskCompleted.AddListener(OnTaskHasCompleted);
                converted.SetTitle(converted.task.gameObject.name);
            }
            grid.UpdateCollection();
        }

        private void Update()
        {
            /*
            foreach (var item in _taskList)
            {
                item.Highlight(item.task.stepState.Equals(BaseTrainingStep.StepState.StepStarted));
            }
            */
        }

        private void OnTaskHasCompleted(BaseTaskItem item)
        {
            GreifbarTaskItem converted = item as GreifbarTaskItem;
            converted.Highlight(false);
        }

        private void OnTaskHasStarted(BaseTaskItem item)
        {
            Debug.Log("On Task Started");
            if(item is GreifbarTaskItem converted) converted.Highlight(true);
            
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
