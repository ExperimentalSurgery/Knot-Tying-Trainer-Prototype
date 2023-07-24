using System;
using UnityEngine;

namespace NMY.VirtualRealityTraining
{
    public class TrainingStepSimpleList : TrainingStepBaseList<SimpleTaskItem>
    {
    }

    [Serializable]
    public class SimpleTaskItem : BaseTaskItem
    {
    }
}