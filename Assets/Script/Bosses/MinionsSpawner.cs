using UnityEngine;
using System.Collections.Generic;

public class MinionsSpawner : MonoBehaviour
{
    public List<GameObject> Enemies;
    public int SpawnRadius = 10;

    private int SpawnDelay = 10;
    private float SpawnTimer = 0;

    private int AmountToSpawn = 1;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance.GameState == GameState.GamePlay)
        {
            if (SpawnTimer >= SpawnDelay)
            {
                SpawnMinions();
                SpawnTimer = 0;
            }
            else
            {
                SpawnTimer += Time.deltaTime;
            }
        }
    }

    void SpawnMinions()
    {
        for (int i = 0; i < AmountToSpawn; i++)
        {
            float randomX = Random.Range(-SpawnRadius, SpawnRadius);
            float randomZ = Random.Range(-SpawnRadius, SpawnRadius);
            Debug.Log("Spawned Enemy!");
            Instantiate(Enemies[Random.Range(0, Enemies.Count)], new Vector3
            {
                x = transform.position.x + randomX,
                y = transform.position.y,
                z = transform.position.z + randomZ,
            }, Quaternion.identity);
        }
    }
}

