using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Unit : MonoBehaviour
{
    public static List<Unit> unitsInGame = new List<Unit>();

    public static float Damage(Unit source, Unit target, int damage, Elements damageType, AttackTypes attackType)
    {
        if (!target.isAlive)
            return 0;

        float incomeDamage = damage * source.GetDamageFactor() + source.GetDamageSummand();
        return target.TakeDamage(incomeDamage, damageType, attackType);
    }

    public static List<Unit> GetUnitsInRange(Vector3 center, float range)
    {
        float X = center.x, Z = center.z;

        List<Unit> unitsInRange = new List<Unit>();
        foreach (var unit in unitsInGame)
        {
            float x = unit.position.x, z = unit.position.z;
            if ((x - X) * (x - X) + (z - Z) * (z - Z) <= range * range)
                unitsInRange.Add(unit);
        }

        return unitsInRange;
    }

    /// <summary> Расстояние между юнитами в плоскости XZ </summary>
    public static float GetSqrFlatDistance(Unit fromUnit, Unit toUnit)
    {
        float xA = fromUnit.position.x; float xB = toUnit.position.x;
        float zA = fromUnit.position.z; float zB = toUnit.position.z;

        return (xB - xA) * (xB - xA) + (zB - zA) * (zB - zA);
    }

    /// <summary> Возвращает угол преобразованный из диапазона [0; 360] в диапазон [-180; 180]</summary>
    public static float ClampAngle(float angle)
    {
        if (angle > 180)
            return -360 + angle;
        else
            return angle;
    }

    /// <summary> Возвращает мировой угол между векторами в диапазоне [0; 360]</summary>
    public static float AngleBetweenVectors(Vector3 vectorA, Vector3 vectorB)
    {
        float scalar = Scalar(vectorA, vectorB);
        float cos = scalar / (vectorA.magnitude * vectorB.magnitude);
        float alpha = Mathf.Acos(cos);
        return alpha;
    }

    /// <summary> Возвращает скалаярное произведение векторов</summary>
    public static float Scalar(Vector3 vectorA, Vector3 vectorB)
    {
        float xA = vectorA.x, xB = vectorB.x;
        float yA = vectorA.y, yB = vectorB.y;
        float zA = vectorA.z, zB = vectorB.z;
        return xA * xB + yA * yB + zA * zB;
    }
}
