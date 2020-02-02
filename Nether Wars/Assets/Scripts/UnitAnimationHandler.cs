using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimationHandler : MonoBehaviour
{
    [Tooltip("Юнит, для которого ведется отслеживание анимаций")]
    public Unit unit;

    [Tooltip("Список частей модели, которым нужно включить физику после гибели юнита")]
    public List<Rigidbody> bodyParts;

    public void CastStart()
    {
        unit.StartCast();
    }

    public void CastEvent()
    {
        unit.Cast();
    }

    public void CastEnd()
    {
        unit.EndCast();
    }

    public void ReleaseBodyParts()
    {
        foreach (Rigidbody rbody in bodyParts)
        {
            rbody.transform.SetParent(this.transform);
            Collider collider = rbody.GetComponent<Collider>();
            collider.enabled = true;
            rbody.isKinematic = false;
            rbody.AddExplosionForce(250, transform.position, 2f);
        }
    }
}
