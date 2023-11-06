using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseRotation : MonoBehaviour
{
    private float y;
    [SerializeField]
    [Range(0, 10)]
    private float sensitivity = 1.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        y = Input.GetAxis("Mouse X");
        transform.eulerAngles = transform.eulerAngles + new Vector3 (0, y * sensitivity, 0);
    }
}
