using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NMY.VirtualRealityTraining.Steps;
using UnityEngine;
using UnityEngine.Events;

namespace NMY.VirtualRealityTraining
{
    public abstract class TrainingStepBaseList<T> : MonoBehaviour where T : BaseTaskItem
    {
        [SerializeField] protected List<T> _taskList = new();

        [Header("Events")]
        public UnityEvent<BaseTaskItem> onTaskStarted;
        public UnityEvent<BaseTaskItem> onTaskFinished;
        public UnityEvent<BaseTaskItem> onTaskCompleted;

        private Dictionary<BaseTrainingStep, T> _dictionary = new();

        private void Awake()
        {
            foreach (var baseTaskItem in _taskList)
            {
                _dictionary.TryAdd(baseTaskItem.task, baseTaskItem);
            }
        }

        protected virtual void OnEnable()
        {
            BaseTrainingStep.OnStepChanged += OnStepChanged;
        }

        protected virtual void OnDisable()
        {
            BaseTrainingStep.OnStepChanged -= OnStepChanged;
        }

        protected virtual void OnStepChanged(object sender, BaseTrainingStepEventArgs args)
        {
            foreach (var taskItem in _taskList)
            {
                if (taskItem.task != args.step) continue;

                args.step.OnStepStarted  -= OnStepStarted;
                args.step.OnStepFinished  -= OnStepFinished;
                args.step.OnStepCompleted -= OnStepCompleted;
                args.step.OnStepStarted  += OnStepStarted;
                args.step.OnStepFinished  += OnStepFinished;
                args.step.OnStepCompleted += OnStepCompleted;
                break;
            }
        }

        private void OnStepStarted(object sender, BaseTrainingStepEventArgs args)
        {
            if (!_dictionary.ContainsKey(args.step)) return;
            
            var item = _dictionary[args.step];
            item.ExecuteTaskStarted();
            onTaskStarted?.Invoke(item);

            args.step.OnStepStarted -= OnStepStarted;
        }

        private void OnStepFinished(object sender, BaseTrainingStepEventArgs args)
        {
            if (!_dictionary.ContainsKey(args.step)) return;
            
            var item = _dictionary[args.step];
            item.ExecuteTaskFinished();
            onTaskFinished?.Invoke(item);
            
            args.step.OnStepFinished -= OnStepFinished;
        }
        
        private void OnStepCompleted(object sender, BaseTrainingStepEventArgs args)
        {
            if (!_dictionary.ContainsKey(args.step)) return;
            
            var item = _dictionary[args.step];
            item.ExecuteTaskCompleted();
            onTaskCompleted?.Invoke(item);
            
            args.step.OnStepCompleted -= OnStepCompleted;
        }
    }

    [Serializable]
    public abstract class BaseTaskItem
    {
        public UnityEvent<BaseTaskItem>   onTaskStarted;
        public UnityEvent<BaseTaskItem>   onTaskFinished;
        public UnityEvent<BaseTaskItem>   onTaskCompleted;
        public BaseTrainingStep           task;

        public virtual void ExecuteTaskStarted()
        {
            onTaskStarted?.Invoke(this);
        }

        public virtual void ExecuteTaskFinished()
        {
            onTaskFinished?.Invoke(this);
        }

        public virtual void ExecuteTaskCompleted()
        {
            onTaskCompleted?.Invoke(this);
        }
    }
}