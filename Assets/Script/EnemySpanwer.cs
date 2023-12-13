using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpanwer : MonoBehaviour
{
    public GameObject Enemy;

    [SerializeField]
    private int Frequency = 5;

    [SerializeField]
    private int AmountToSpawn = 10;

    private int Spawned = 0;

    private float timer;

    private void Awake()
    {
        timer = Frequency;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (timer <= 0 && Spawned < AmountToSpawn)
        {
            Instantiate(Enemy, new Vector3(16.23f, -1.03f, 9.26f), Quaternion.identity);
            Spawned++;
            timer = Frequency;
        }

        timer -= Time.deltaTime;
    }
}
