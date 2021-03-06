﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirefighterUnit : BaseUnit {

    public GameObject deathEffect;
    public GameObject waterSprayEffect;
    [SerializeField]
    private Animator anim;
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();

        

        if (sprayWater && waterAmount >0 && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 50, targetMask))
            {              
                ShootWater(hit.point);
            }
        }
        if (isSelected && Input.GetKeyDown(KeyCode.X))
        {
            sprayWater = !sprayWater;

            if (sprayWater == false)
            {
                waterSprayEffect.SetActive(false);
            }
        }

        if (navMeshAgent.remainingDistance<=navMeshAgent.stoppingDistance)
        {
            SetAnimationState(0);
        }

    }

    public override void Move(Vector3 destination)
    {       
        base.Move(destination);
        sprayWater = false;
        waterSprayEffect.SetActive(false);

        SetAnimationState(1);
        Debug.Log(navMeshAgent);

    }

    public override void ShootWater(Vector3 target)
    {
        if (Vector3.Distance(transform.position, target) < range)
        {
            Debug.Log("In Range");
            Collider[] thingsOnFire = Physics.OverlapSphere(target, waterRadius,burnableMask);
            waterSprayEffect.SetActive(sprayWater);
            
            SpawnWater(target);
            transform.rotation = Quaternion.LookRotation(transform.position - target, transform.up); 
            
            if (thingsOnFire != null)
            {
                int firesPutout = 0;
                for (int i = 0; i < thingsOnFire.Length; i++)
                {
                    if (thingsOnFire[i].GetComponent<Burnable>() != null)
                    {
                        if (thingsOnFire[i].GetComponent<Burnable>().isOnFire)
                            thingsOnFire[i].GetComponent<Burnable>().isOnFire = false;
                        else
                        {
                            thingsOnFire[i].GetComponent<Burnable>().moistureAmount = 100;
                        }

                        firesPutout++;
                    }
                  
                }
                Debug.Log("Fires put out " + firesPutout);
            }
            waterAmount--;
        }
        else
        {

           // waterSprayEffect.SetActive(false);
           // sprayWater = false;
        }
    }

    public override void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            OnDeath();
        }
    }

    protected override void OnDeath()
    {
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        
    }

    void SetAnimationState(int stateIndex)
    {
        if (anim != null)
        {
            anim.SetInteger("StateIndex",stateIndex);
        }
    }

    IEnumerator Dying()
    {

        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
