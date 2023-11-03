using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseRotation : MonoBehaviour
{
    private float y;
    [SerializeField]
    [Range(0, 10)]
    private float sensitivity = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        y = Input.GetAxis("Mouse X");
        transform.eulerAngles = transform.eulerAngles + new Vector3 (0, y * sensitivity, 0);
    }
}
