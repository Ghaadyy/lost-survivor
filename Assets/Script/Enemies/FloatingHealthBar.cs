using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;

    private float currentHealth;
    private float maxHealth;
    private float minHealth;

    private bool isDead;
    //public void UpdateHealthBar(float currentValue, float maxValue)
    //{
    //    slider.value = currentValue / maxValue;
    //}

    private void Start()
    {
        currentHealth = maxHealth = 100;
        minHealth = 0;

        slider.maxValue = maxHealth;
        slider.minValue = minHealth;
        slider.value = currentHealth;

        isDead = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
        transform.position = target.position + offset;
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        slider.value = currentHealth;
    }

    public float GetHealthBar()
    {
        return currentHealth;
    }

    public void SetHealthBar(float value)
    {
        currentHealth += value;
        if(currentHealth <= minHealth)
        {
            currentHealth = 0;
            isDead = true;
        }
        else if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    public bool CheckIfDead()
    {
        return isDead;
    }
}
