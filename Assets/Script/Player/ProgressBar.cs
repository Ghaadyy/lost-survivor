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
        counter = PlayerPrefs.GetInt("xpCounter", 1);
        minimum = PlayerPrefs.GetInt("minXP", 0);
        current = PlayerPrefs.GetInt("XP", 0);
        maximum = PlayerPrefs.GetInt("maxXP", 100);
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
        float fillBar = (float)currentOffset / maximumOffset;
        mask.fillAmount = fillBar;
    }

    public void SetCounter(int value) => counter = value;

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

        PlayerPrefs.SetInt("XP", current);
        PlayerPrefs.SetInt("minXP", minimum);
        PlayerPrefs.SetInt("maxXP", maximum);
        PlayerPrefs.SetInt("xpCounter", counter);
    }
}
