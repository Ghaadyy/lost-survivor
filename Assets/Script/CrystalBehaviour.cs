using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalBehaviour : MonoBehaviour
{
    public enum BuffType
    {
        Speed, Strength
    }

    public BuffType buffType;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            var player = other.gameObject.GetComponent<HumanBehaviour>();

            switch (buffType)
            {
                case BuffType.Speed:
                    Debug.Log("Speed Buff !");
                    player.SpeedMultiplier = 1.5f;
                    break;
                case BuffType.Strength:
                    Debug.Log("Strength Buff !");
                    // Strength multiplier here or any other ability
                    break;
            }

            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}