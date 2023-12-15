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

    //private string originalImageColor = "FFFFFF";
    //private string onCooldownImageColor = "5A5A5A";

    private const int abilitiesCount = 5;
    private float[] abilitiesCooldown;
    private float[] currentAbilityCooldown;

    private bool inCombat;
    private float inCombatCooldown;

    //private int comboCount;
    //private bool isLastComboValid;
    void Start()
    {
        animator = GetComponent<Animator>();
        abilitiesImages = GameObject.FindGameObjectsWithTag("Ability").OrderBy(a => a.name).ToArray(); //Get all spells images
        cooldownText = GameObject.FindGameObjectsWithTag("Cooldown").OrderBy(t => t.name).ToArray(); //Get all cooldown text for each spell
        
        abilitiesCooldown = new float[abilitiesCount] { 10.0f, 10.0f, 10.0f, 10.0f, 10.0f }; //Original cooldown of each ability
        currentAbilityCooldown = new float[abilitiesCount] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }; //Current cooldown of the ability
        for(int i=0; i<abilitiesCount; i++)
        {
            Debug.Log(abilitiesImages[i].name + " " + cooldownText[i].name);
        }

        inCombat = false;
        inCombatCooldown = 0.0f;

        //comboCount = 0;
        //isLastComboValid = false;
    }

    void Update()
    {
        UpdateCooldown();
        UpdateOnCombatStatus();
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

            animator.SetBool("Attack", true);
            inCombat = true;
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
        if (Input.GetKeyDown(KeyCode.Alpha1) && currentAbilityCooldown[0] <= 0) //If ability ready then execute it and reset its cooldown counter
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
        for(int i=0; i<abilitiesCount; i++) //For each ability do the following
        {
            if (currentAbilityCooldown[i] > 0) //Decrement ability cooldown if possible
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

    private void UpdateOnCombatStatus()
    {
        if (inCombat)
        {
            ResetCombatCooldown();
        }

        if(inCombatCooldown > 0)
        {
            inCombatCooldown -= Time.deltaTime;
        }
    }

    private void ResetCombatCooldown()
    {
        inCombatCooldown = 5.0f;
    }





    //Animation events

    private void DisableNextComboAttack()
    {
        animator.SetBool("Attack", false);
    }







    //private void ToggleLastCombo()
    //{
    //    animator.SetBool("LastCombo", isLastComboValid);
    //    isLastComboValid = !isLastComboValid;
    //}
}
