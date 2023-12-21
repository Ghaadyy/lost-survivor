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
        LockCursor();
    }

    void Update()
    {
        y = Input.GetAxis("Mouse X");
        transform.eulerAngles = transform.eulerAngles + new Vector3 (0, y * sensitivity, 0);
    }

    public static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
