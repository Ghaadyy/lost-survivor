using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider slider;

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
        current += value;
        if (current < min)
        {
            current = min;
        }
        else if(current > max)
        {
            current = max;
        }
    }

    public float GetMaxHealth()
    {
        return max;
    }
}
