﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//this script manages object interaction, resource gathering and resource expenditure.

public class ScoreManagerAndInteraction : MonoBehaviour
{
    public AudioClip PickUpCoinClip;
    public AudioSource PickUpCoinSource;



    public float Score = 0;
    public float Cash = 0;
    public float Bank = 0;
    private float Price = 0;
    public float Damage;

    //win/lose
    public GameObject WinController;
    public GameObject MuggerSpawnerObj;
    public GameObject HitDisplay;


    //interaction
    private float progressTimer = 0;
    public GameObject ProgressBar;
    public GameObject ProgressBarGreen;
    public GameObject ProgressBarGrey;
    public GameObject CollidedObject;
    public GameObject InteractingObject;



    private IEnumerator buildingCoroutine;



    public bool Interacting;

    // Use this for initialization

    void Start()
    {
        Score = 0;
        Cash = 0;
        Bank = 0;

        StartCoroutine(BankInterest());
        PickUpCoinSource.clip = PickUpCoinClip;

    }

    // Update is called once per frame
    void Update()
    {
        //Score = Cash + Bank;

    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.name);
        progressTimer = 0;
        CollidedObject = collision.gameObject;

        bool valid = false;

        //check what we've hit and then call the relavent function

        switch (collision.tag)
        {
            case "Coin":
                CoinGet();
                //Probably not efficient to destroy, but can replace with pool later
                CollidedObject.SetActive(false);
                break;

            case "Bank":
                //deposit money at a rate
                if (Cash > 0)
                {
                    valid = true;
                }
                break;

            case "Property":
                //purchase property

                Price = CollidedObject.GetComponent<PropertyProperties>().Price;

                if (Score >= Price)
                {
                    valid = true;
                }
                break;

            case "Agency":
                //hire agent

                Price = CollidedObject.GetComponent<AgentManager>().Price;

                if (Score >= Price)
                {
                    valid = true;
                }
                break;

            case "Purchased":
                //check damage

                var DamageType = CollidedObject.GetComponent<PropertyProperties>().damageType;
                Damage = CollidedObject.GetComponent<PropertyProperties>().Damage;
                Price = Damage / 2;

                if (Score >= Price && DamageType != PropertyProperties.DamageType.None)
                {
                    valid = true;
                }
                break;

            case "Enemy":
                //what happens when we hit an enemy?
                WinController.GetComponent<LossCondition>().hits++;
                collision.gameObject.GetComponent<Mugger>().target = MuggerSpawnerObj.transform;
                MuggerSpawnerObj.GetComponent<MuggerSpawner>().SendAway();
                Cash -= 40;
                HitDisplay.SetActive(true);
                

                break;

        }
        //carries out building action
        if (collision.tag != "Coin" && valid == true)
        {
            Interacting = true;
            InteractingObject = collision.gameObject;
            StartCoroutine(BuildingAction(collision.tag));
        }

    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (Interacting == true && (collision.tag == "Bank" || collision.tag == "Property" || collision.tag == "Agency" ))
        {
            Interacting = false;
        }
    }


    public void CoinGet()
    {
        //increases score
        Cash = Cash + 10;
        Score = Cash + Bank;
        PickUpCoinSource.Play();
    }

    private void Interact(string BuildingType)
    {
        //Carries out investment action

        switch (BuildingType)
        {
            case "Bank":
                Bank += Cash;
                Cash = 0;
                AddUpScore();
                break;

            case "Property":
                //purchases property
                if (Score >= Price)
                {
                    if (Price > Cash)
                    {
                        Price -= Cash;
                    }
                    else
                    {
                        Cash -= Price;
                        Price = 0;
                    }

                    if (Price > 0)
                    {
                        Bank -= Price;
                    }

                    //update values
                    AddUpScore();
                    InteractingObject.tag = "Purchased";
                    InteractingObject.GetComponent<PropertyProperties>().Purchased();

                }
                else
                {
                    Debug.Log("Cannot afford action, Called incorrectly?");
                }
                break;

            case "Agency":
                //hire agent
                if (Score >= Price)
                {
                    if (Price > Cash)
                    {
                        Price -= Cash;
                    }
                    else
                    {
                        Cash -= Price;
                        Price = 0;
                    }

                    if (Price > 0)
                    {
                        Bank -= Price;
                    }

                    //update values
                    AddUpScore();
                    InteractingObject.GetComponent<AgentManager>().HireAgent();

                }
                else
                {
                    Debug.Log("Cannot afford action, Called incorrectly?");
                }
                break;

            case "Purchased":
                //repair

                //check item (not implemented yet)
                if (Score >= Price)
                {
                    if (Price > Cash)
                    {
                        Price -= Cash;
                    }
                    else
                    {
                        Cash -= Price;
                        Price = 0;
                    }

                    if (Price > 0)
                    {
                        Bank -= Price;
                    }

                    //update values
                    AddUpScore();
                    InteractingObject.GetComponent<PropertyProperties>().damageType = PropertyProperties.DamageType.None;

                }
                else
                {
                    Debug.Log("Cannot afford action, Called incorrectly?");
                }
                break;





        }
    }

    private IEnumerator BuildingAction(string building)
    {
        //progress bar setup
        bool actionComplete = false;
        float Progress = 0;
        float requiredProgress = 0;
        float progressBar = 0;
        float progressModifier = 200;
        float distanceX = ProgressBarGreen.transform.localPosition.x - ProgressBarGrey.transform.localPosition.x;
        Vector3 initialPosGreen = ProgressBarGreen.transform.localPosition;
        ProgressBar.SetActive(true);
        ProgressBarGreen.transform.localScale = new Vector3(0.1f, ProgressBarGreen.transform.localScale.y);

        //determines type of action and parameters
        switch (building)
        {
            case "Bank":
                requiredProgress = Cash;
                break;

            case "Property":
                requiredProgress = Price;
                break;

            case "Agency":
                requiredProgress = Price;
                break;

            case "Purchased":
                requiredProgress = Damage;
                break;
        }

        //create loop to start progress
        while (actionComplete == false)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            if (Interacting == true)
            {

                //action
                Progress = Progress + Time.deltaTime * progressModifier;
                progressModifier++;
                progressBar = Progress / requiredProgress;
                Debug.Log(progressBar * 100);

                ProgressBarGreen.transform.localScale = new Vector3(progressBar * ProgressBarGrey.transform.localScale.x, ProgressBarGreen.transform.localScale.y);
                ProgressBarGreen.transform.localPosition = new Vector3(initialPosGreen.x - (distanceX * progressBar), ProgressBarGreen.transform.localPosition.y);

                //test if action is complete
                if (Progress >= requiredProgress)
                {
                    //complete action, reset loading bar
                    Interact(building);
                    ProgressBar.SetActive(false);
                    ProgressBarGreen.transform.localPosition = initialPosGreen;

                    actionComplete = true;
                };

            }
            else
            {
                //resets if interaction is canceled
                ProgressBar.SetActive(false);
                ProgressBarGreen.transform.localPosition = initialPosGreen;
                actionComplete = true;
            }


        }

        yield return null;
    }

    private IEnumerator BankInterest()
    {
        //increases bank value relative to bank value
        bool coroutineRun = true;

        while (coroutineRun)
        {
            yield return new WaitForSecondsRealtime(5f);

            Bank = Bank + (Bank * 0.01f);
            Bank = (float)Math.Round(Bank, 2);
            AddUpScore();


        }
    }

    public void AddUpScore()
    {
        Score = Cash + Bank;
    }
}
