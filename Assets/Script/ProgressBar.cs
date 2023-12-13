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

    private int counter;
    void Start()
    {
        counter = 1;
        minimum = current = 0;
        maximum = 100;
    }
    void Update()
    {
        IncrementXP();
        FillProgressBar();
    }

    void FillProgressBar()
    {
        float currentOffset = current - minimum;
        float maximumOffset = maximum - minimum;
        float fillBar = (float)currentOffset/maximumOffset;
        mask.fillAmount = fillBar;
    }

    void IncrementXP()
    {
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            current += 20;
            if(current >= maximum)
            {
                minimum = maximum;
                maximum += 250 * counter;
                counter++;
            }
        }
    }
}
