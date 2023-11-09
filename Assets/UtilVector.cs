using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameHelper
{
    public static Vector2 CalculatePerpendicular(Vector2 vector, bool right = false)
    {
        // 计算向量在x轴上的投影长度
        float x = vector.x;

        // 计算向量在y轴上的投影长度
        float y = vector.y;

        // 根据投影长度计算垂直于向量的向量坐标
        float newX = -y;
        float newY = x;

        if (right)
        {
            newX = y;
            newY = -x;
        }

        // 将结果存储到新的Vector2对象中并返回
        return new Vector2(newX, newY);
    }

    public static float VectorCalAngle(Vector2 vectorA, Vector2 vectorB)
    {
        vectorA = vectorA.normalized;
        vectorB = vectorB.normalized;

        float dotProduct = Vector2.Dot(vectorA, vectorB);
        return MathF.Acos(dotProduct) * (180 / MathF.PI);
    }

    public static void VectorTruncate(ref Vector2 vector, float max)
    {
        var distance = MathF.Min(vector.magnitude, max);

        vector = vector.normalized * distance;
    }

}
