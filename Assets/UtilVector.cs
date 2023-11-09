using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameHelper
{
    public static Vector2 CalculatePerpendicular(Vector2 vector, bool right = false)
    {
        // ����������x���ϵ�ͶӰ����
        float x = vector.x;

        // ����������y���ϵ�ͶӰ����
        float y = vector.y;

        // ����ͶӰ���ȼ��㴹ֱ����������������
        float newX = -y;
        float newY = x;

        if (right)
        {
            newX = y;
            newY = -x;
        }

        // ������洢���µ�Vector2�����в�����
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
