using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    private Slider slider;

    private float currentHealth;
    private float maxHealth;
    private float minHealth;

    private bool isDead;

    private void Start()
    {
        slider = gameObject.GetComponent<Slider>();
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
        if (GameManager.Instance.GameState == GameState.GamePlay)
        {
            UpdateHealthBar();
        }
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
        if (currentHealth <= minHealth)
        {
            currentHealth = 0;
            isDead = true;
        }
        else if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    public bool CheckIfDead()
    {
        return isDead;
    }
}
