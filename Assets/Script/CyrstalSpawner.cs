using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyrstalSpawner : MonoBehaviour
{
    public int Amount = 50;
    public int Range = 400;

    public List<GameObject> Crystals;

    public LayerMask whatIsGround;

    private void Awake()
    {
        while (Amount != 0)
        {
            float x = Random.Range(-Range, Range);
            float z = Random.Range(-Range, Range);

            Vector3 point = new Vector3(
                transform.position.x + x,
                50,
                transform.position.z + z);

            if (Physics.Raycast(point, -transform.up, out RaycastHit hit, whatIsGround))
            {
                Instantiate(Crystals[Random.Range(0, 3)], hit.point, Quaternion.identity);
                Amount--;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
