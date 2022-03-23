using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MiscMath
{
    /// <summary>
    /// Clamps a Vector3 value in between a min and max value, e.g. for constraining a position inside bounds.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static Vector3 Vector3Clamp(Vector3 value, Vector3 min, Vector3 max)
    {
        value.x = Mathf.Clamp(value.x, min.x, max.x);
        value.y = Mathf.Clamp(value.y, min.y, max.y);
        value.z = Mathf.Clamp(value.z, min.z, max.z);
        return value;
    }
    /// <summary>
    /// The minimum of a Vector3's three values.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static float Vector3Min(Vector3 value) => Mathf.Min(Mathf.Min(value.x, value.y), value.z);
    /// <summary>
    /// The maximum of a Vector3's three values.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static float Vector3Max(Vector3 value) => Mathf.Max(Mathf.Max(value.x, value.y), value.z);
}
