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
    public GameObject SpellCastPoint, FireSphere;

    private Animator animator;
    private HealthBar healthBar;

    private GameObject[] abilitiesImages;
    private GameObject[] cooldownText;

    //private string originalImageColor = "FFFFFF";
    //private string onCooldownImageColor = "5A5A5A";

    private const int abilitiesCount = 5;
    private float[] abilitiesCooldown;
    private float[] currentAbilityCooldown;

    private int buffsCount = 3; //Strength, Healing, Protection
    private float[] buffsCooldown;

    private float damage = 10.0f;
    private float strength = 1.0f;
    private float heal = 0.1f;
    private float protection = 0.0f;

    private bool isDead = false;
    private bool isImmune = false;

    private float explosionForce = 20.0f;
    private float explosionRadius = 10.0f;

    public bool CheckIfDead()
    {
        return isDead;
    }

    public bool CheckIfImmune()
    {
        return isImmune;
    }

    public float GetPlayerDamage()
    {
        return strength * damage;
    }

    private void Buff_UpdateStrength()
    {
        if (buffsCooldown[0] > 0)
        {
            if (strength != 2) strength = 2;
        }
        else
        {
            if(strength != 1) strength = 1;
        }
    }

    private void Buff_UpdateHealth()
    {
        if (buffsCooldown[1] > 0)
        {
            healthBar.SetHealth(healthBar.GetMaxHealth() * heal * Time.deltaTime);
        }
    }

    private void Immune_Update()
    {
        if (buffsCooldown[2] > 0)
        {
            if(!isImmune) isImmune = true;
        }
        else
        {
            if (isImmune) isImmune = false;
        }
    }

    //private bool inCombat;
    //private float inCombatCooldown;

    //private int comboCount;
    //private bool isLastComboValid;
    void Start()
    {
        animator = GetComponent<Animator>();
        healthBar = GameObject.FindObjectOfType<HealthBar>();

        abilitiesImages = GameObject.FindGameObjectsWithTag("Ability").OrderBy(a => a.name).ToArray(); //Get all spells images
        cooldownText = GameObject.FindGameObjectsWithTag("Cooldown").OrderBy(t => t.name).ToArray(); //Get all cooldown text for each spell
        
        abilitiesCooldown = new float[abilitiesCount] { 10.0f, 10.0f, 10.0f, 10.0f, 10.0f }; //Original cooldown of each ability
        currentAbilityCooldown = new float[abilitiesCount] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }; //Current cooldown of the ability

        buffsCooldown = new float[buffsCount]; //Buffs cooldown

        //inCombat = false;
        //inCombatCooldown = 0.0f;

        //comboCount = 0;
        //isLastComboValid = false;
    }

    void Update()
    {
        UpdateAbilitiesCooldown();
        UpdateBuffsCooldown();
        Buff_UpdateStrength();
        Buff_UpdateHealth();
        Immune_Update();

        if (!isDead)
        {
            Heal();
            Buff();
            CastSpell();
            Immune();
            ShakeGround();
            Punch();
            Block();
        }

        Die();

    }

    void Punch()
    {
        if (Input.GetMouseButtonDown(0))
        {

            animator.SetBool("Attack", true);
            //inCombat = true;
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
            healthBar.SetHealth(healthBar.GetMaxHealth() * 0.25f); //Heals 25% of max health
        }
    }

    void Buff()
    {
        if(Input.GetKeyDown(KeyCode.Alpha2) && currentAbilityCooldown[1] <= 0) 
        {
            animator.SetTrigger("Buff");
            currentAbilityCooldown[1] = abilitiesCooldown[1];
            buffsCooldown[0] = 10.0f; //Apply strength buff
            buffsCooldown[1] = 10.0f; //Apply healing overtime buff
            Debug.Log("Strength buff activated");
            Debug.Log("Healing buff activated");
        }
    }

    void CastSpell()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3) && currentAbilityCooldown[2] <= 0)
        {
            animator.SetTrigger("Cast");
        }
    }

    void Immune()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4) && currentAbilityCooldown[3] <= 0)
        {
            isImmune = true;
            buffsCooldown[2] = 10.0f; //Apply immune factor
            currentAbilityCooldown[3] = abilitiesCooldown[3];
        }
    }

    void ShakeGround()
    {
        if (Input.GetKeyDown(KeyCode.Alpha5) && currentAbilityCooldown[4] <= 0)
        {
            addExplosionForce();
            currentAbilityCooldown[4] = abilitiesCooldown[4];
        }
    }

    void Die()
    {
        if(healthBar.GetHealth() == 0)
        {
            isDead = true;
            animator.SetBool("Die", isDead);
        }
    }

    void Revive()
    {
        if (isDead)
        {
            isDead = false;
            animator.SetBool("Die", isDead);
        }
    }

    void addExplosionForce()
    {
        // Detect all colliders within the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider col in colliders)
        {
            // Check if the object has a rigidbody
            if (col.gameObject.tag == "Enemy" && col.TryGetComponent(out Rigidbody rb))
            {
                // Apply explosion force to the rigidbody
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0.0f, ForceMode.Impulse);
                rb.GetComponentInChildren<FloatingHealthBar>().SetHealthBar(-explosionForce);
            }
        }
    }

    void UpdateAbilitiesCooldown()
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

    void UpdateBuffsCooldown()
    {
        for (int i = 0; i < buffsCount; i++)
        {
            if (buffsCooldown[i] > 0)
            {
                buffsCooldown[i] -= Time.deltaTime;
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

    //private void UpdateOnCombatStatus()
    //{
    //    if (inCombat)
    //    {
    //        ResetCombatCooldown();
    //    }

    //    if(inCombatCooldown > 0)
    //    {
    //        inCombatCooldown -= Time.deltaTime;
    //    }
    //}

    //private void ResetCombatCooldown()
    //{
    //    inCombatCooldown = 5.0f;
    //}





    //Animation events

    private void DisableNextComboAttack()
    {
        animator.SetBool("Attack", false);
    }

    private void ReleaseSpell()
    {
        GameObject sphere = Instantiate(FireSphere, SpellCastPoint.transform.position, transform.rotation);
        sphere.GetComponent<Rigidbody>().AddForce(transform.forward * 1000);
        currentAbilityCooldown[2] = abilitiesCooldown[2];
    }





    //private void ToggleLastCombo()
    //{
    //    animator.SetBool("LastCombo", isLastComboValid);
    //    isLastComboValid = !isLastComboValid;
    //}
}
