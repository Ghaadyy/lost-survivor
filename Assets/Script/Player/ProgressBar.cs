using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public int minimum;
    public int maximum;
    public int current;

    public Image mask;
    public Image fill;

    private HealthBar playerHealthBar;
    private HumanCombat playerCombat;

    private int counter;
    void Start()
    {
        counter = 1;
        minimum = current = 0;
        maximum = 100;
        playerHealthBar = FindAnyObjectByType<HealthBar>();
        playerCombat = FindAnyObjectByType<HumanCombat>();
    }
    void Update()
    {
        FillProgressBar();
    }

    void FillProgressBar()
    {
        float currentOffset = current - minimum;
        float maximumOffset = maximum - minimum;
        float fillBar = (float)currentOffset/maximumOffset;
        mask.fillAmount = fillBar;
    }

    public void IncrementXP(int value) //add xp to the progess bar
    {
        current += value;
        if (current >= maximum)
        {
            minimum = maximum;
            maximum += 75 * counter;
            counter++;
            playerHealthBar.SetMaxHealth(playerHealthBar.GetMaxHealth() + 35); //Increment player health bar
            playerHealthBar.SetHealth(playerHealthBar.GetMaxHealth());

            playerCombat.SetPlayerStrength(0.25f); //Increment player strength
            Debug.Log("Player strength: " + playerCombat.GetPlayerStrength());

            playerCombat.IncrementPlayerLevel();
            Debug.Log("Player level: " + playerCombat.GetPlayerLevel());

        }
    }
}
