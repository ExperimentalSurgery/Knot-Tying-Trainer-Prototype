using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace NMY.VirtualRealityTraining
{
    public class TrainingStepLocalizedStringList : TrainingStepBaseList<LocalizedStringTaskItem>
    {
        private void Start()
        {
            foreach (var item in _taskList)
            {
                item.SetName();
            }
        }
    }

    [Serializable]
    public class LocalizedStringTaskItem : BaseTaskItem
    {
        [SerializeField] private LocalizedString _taskName;
        [SerializeField] private TMP_Text        _text;

        public string taskName => _taskName.GetLocalizedString();

        public TMP_Text text => _text;

        public void SetName()
        {
            if (_text == null) return;
            _text.text = taskName;
        }
    }
}