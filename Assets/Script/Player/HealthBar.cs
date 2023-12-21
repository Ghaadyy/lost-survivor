using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider slider;
    private HumanCombat player;

    public float min = 0;
    public float max = 100;
    public float current;

    public int damage;

    void Start()
    {
        current = max;
        slider = gameObject.GetComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = current;

        damage = 10;

        player = FindObjectOfType<HumanCombat>();
    }

    void Update()
    {
        TakeDamage();
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (slider.value != current)
        {
            slider.value = current;
        }
    }

    private void TakeDamage() //Test health bar
    {
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            current -= damage;
            if (current < 0)
            {
                current = min;
            }
        }
    }

    public void SetHealth(float value)
    {
        if (player.CheckIfImmune() && value < 0) return;

        current += value;
        if (current < min)
        {
            current = min;
        }
        else if(current > max)
        {
            Debug.Log("Player health: " + current);
            current = max;
        }
    }

    public float GetHealth()
    {
        return current;
    }

    public void SetMaxHealth(float value)
    {
        max = value;
    }
    public float GetMaxHealth()
    {
        return max;
    }

}
