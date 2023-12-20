using UnityEngine;
using System.Collections;

public class csDestroyEffect : MonoBehaviour {

	void Update ()
    {
        Destroy(gameObject, 10.0f);
    }
}
