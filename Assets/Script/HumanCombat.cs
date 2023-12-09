using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class HumanCombat : MonoBehaviour
{
    private Animator animator;

    private GameObject[] abilitiesImages;
    private GameObject[] cooldownText;

    private string originalImageColor = "FFFFFF";
    private string onCooldownImageColor = "5A5A5A";

    private const int abilitiesCount = 5;
    private float[] abilitiesCooldown;
    private float[] currentAbilityCooldown;
    void Start()
    {
        animator = GetComponent<Animator>();
        abilitiesImages = GameObject.FindGameObjectsWithTag("Ability").OrderBy(a => a.name).ToArray();
        cooldownText = GameObject.FindGameObjectsWithTag("Cooldown").OrderBy(t => t.name).ToArray();
        
        abilitiesCooldown = new float[abilitiesCount] { 10.0f, 10.0f, 10.0f, 10.0f, 10.0f };
        currentAbilityCooldown = new float[abilitiesCount] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        for(int i=0; i<abilitiesCount; i++)
        {
            Debug.Log(abilitiesImages[i].name + " " + cooldownText[i].name);
        }
    }

    void Update()
    {
        UpdateCooldown();
        Heal();
        Buff();
        CastSpell();
        Immune();
        ShakeGround();
        Punch();
        Block();
    }

    void Punch()
    {
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("PunchL");
        }
    }

    void Block()
    {
        if (Input.GetMouseButtonDown(1))
        {
            animator.SetBool("Block", true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            animator.SetBool("Block", false);
        }
    }

    void Heal()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && currentAbilityCooldown[0] <= 0)
        {
            currentAbilityCooldown[0] = abilitiesCooldown[0];
        }
    }

    void Buff()
    {
        if(Input.GetKeyDown(KeyCode.Alpha2) && currentAbilityCooldown[1] <= 0) 
        {
            animator.SetTrigger("Buff");
            currentAbilityCooldown[1] = abilitiesCooldown[1];
        }
    }

    void CastSpell()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3) && currentAbilityCooldown[2] <= 0)
        {
            currentAbilityCooldown[2] = abilitiesCooldown[2];
        }
    }

    void Immune()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4) && currentAbilityCooldown[3] <= 0)
        {
            currentAbilityCooldown[3] = abilitiesCooldown[3];
        }
    }

    void ShakeGround()
    {
        if (Input.GetKeyDown(KeyCode.Alpha5) && currentAbilityCooldown[4] <= 0)
        {
            currentAbilityCooldown[4] = abilitiesCooldown[4];
        }
    }

    void UpdateCooldown()
    {
        for(int i=0; i<abilitiesCount; i++)
        {
            if (currentAbilityCooldown[i] > 0)
            {
                currentAbilityCooldown[i] -= Time.deltaTime;
                ChangeImageColor(abilitiesImages[i].GetComponent<Image>(), Color.gray);
                ChangeCooldownText(cooldownText[i].GetComponent<TMP_Text>(), i);
            }
            else
            {
                ChangeImageColor(abilitiesImages[i].GetComponent<Image>(), Color.white);
                ChangeCooldownText(cooldownText[i].GetComponent<TMP_Text>());

            }
        }
    }

    private void ChangeImageColor(Image image, Color color)
    {
        image.color = color;
    }

    private void ChangeCooldownText(TMP_Text cd, int idx)
    {
        cd.text = Convert.ToInt32(currentAbilityCooldown[idx]).ToString();
    }

    private void ChangeCooldownText(TMP_Text cd)
    {
        cd.text = "";
    }
}
