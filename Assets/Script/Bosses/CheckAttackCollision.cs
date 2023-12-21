using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckAttackCollision : MonoBehaviour
{
    public GameObject Sphere;
    public Transform HitPoint;

    void CreateHitPoint()
    {
        Debug.Log("Creating hit point");
        Instantiate(Sphere, HitPoint.position, Quaternion.identity);
    }
}