using BehaviorDesigner.Runtime;
using UnityEngine;

// 自定义共享变量类型，用于存储 float 数组
[System.Serializable]
public class SharedFloatArray : SharedVariable<float[]>
{
    public static implicit operator SharedFloatArray(float[] value)
    {
        var sharedFloatArray = new SharedFloatArray();
        sharedFloatArray.Value = value;
        return sharedFloatArray;
    }

    public override string ToString()
    {
        if (Value == null)
        {
            return "null";
        }
        return string.Join(", ", Value);
    }

    public override object GetValue()
    {
        return Value;
    }

    public override void SetValue(object value)
    {
        if (value is float[])
        {
            Value = (float[])value;
        }
    }
}