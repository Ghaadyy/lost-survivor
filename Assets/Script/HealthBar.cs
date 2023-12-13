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

    // Start is called before the first frame update
    void Start()
    {
        current = max;
        slider = gameObject.GetComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = current;

        damage = 10;
    }

    // Update is called once per frame
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

    private void TakeDamage()
    {
        if(Input.GetKeyDown(KeyCode.Alpha6)) {
            current -= damage;
            if(current < 0)
            {
                current = min;
            }
        }
    }
}
