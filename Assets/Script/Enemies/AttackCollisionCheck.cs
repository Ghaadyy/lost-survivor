using UnityEngine;
using System.Collections;

public class AttackCollisionCheck : MonoBehaviour
{
    public GameObject Sphere;
    public Transform HitPoint;

    void CreateHitPoint()
    {
        Instantiate(Sphere, HitPoint.position, Quaternion.identity);
    }
}

