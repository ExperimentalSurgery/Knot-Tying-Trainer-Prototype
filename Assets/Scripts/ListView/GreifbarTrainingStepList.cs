using System;
using Microsoft.MixedReality.Toolkit.Utilities;
using NMY.VirtualRealityTraining;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace DFKI.NMY
{
    public class GreifbarTrainingStepList : TrainingStepBaseList<GreifbarTaskItem>
    {
        [SerializeField] private GestureStepListEntry rowEntryPrefab;
        [SerializeField] private VerticalLayoutGroup grid;
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
                converted.Highlight(item.task.stepState.Equals(BaseTrainingStep.StepState.StepFinished));
                
                GreifbarChapter chapter = converted.task as GreifbarChapter;
                if(chapter) {
                    if(!chapter.ChapterTitle.IsEmpty)converted.SetTitle(chapter.ChapterTitle.GetLocalizedString());
                    if(chapter.ChapterIcon) converted.SetIcon(chapter.ChapterIcon);
                }
                
            }
            
        }

        private void Update()
        {
            foreach (var item in _taskList) {
                GreifbarTaskItem converted = item as GreifbarTaskItem;
                converted.Highlight(item.task.stepState.Equals(BaseTrainingStep.StepState.StepFinished));
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
        public void SetIcon(Image icon) => listItem.SetIcon(icon);
    }
    
}
