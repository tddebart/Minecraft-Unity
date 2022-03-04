using System;
using UnityEngine;
     
[AttributeUsage (AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class RangeExAttribute : PropertyAttribute
{
    public readonly int min;
    public readonly int max;
    public readonly int step;
     
    public RangeExAttribute (int min, int max, int step)
    {
        this.min = min;
        this.max = max;
        this.step = step;
    }
}