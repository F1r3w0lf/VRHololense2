using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserPlayer : Player {



	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {



       
        /*
        if (GameManager.instance.players[GameManager.instance.currentPlayerIndex] == this)
        {
            transform.GetComponent<Renderer>().material.color = Color.blue;
        }
        else
        {
            transform.GetComponent<Renderer>().material.color = Color.blue;
        } */


        if (HP<= 0)
        {
            transform.rotation = Quaternion.Euler(new Vector3(90,0,0));
            //transform.GetComponent<Renderer>().material.color = Color.red;
        }
    }


    public override void TurnUpdate()
    {
        if (Vector3.Distance(moveDestination, transform.position) > 0.1f){
            transform.position += (moveDestination - transform.position).normalized * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(moveDestination, transform.position) <= 0.1f){
               transform.position = moveDestination;
               GameManager.instance.NextTurn();
                actionPoints--;
            }
        }
        

        base.TurnUpdate();
    }

    public override void TurnOnGUI()
    {
        float buttonHeight = 50;
        float buttonWidth = 150;
        Rect buttonRect = new Rect(0, Screen.height - buttonHeight * 3,buttonWidth, buttonHeight);

        //move button

        if (GUI.Button(buttonRect, "Move"))
        {
            if (!moving)
            {
                moving = true;
                attacking = false;
            }
            else
            {
                moving = false;
                attacking = false;
            }
        }

        //Attack Button
        buttonRect = new Rect(0, Screen.height - buttonHeight * 2, buttonWidth, buttonHeight);

        if (GUI.Button(buttonRect, "Attack"))
        {
            if (!attacking)
            {
                moving = false;
                attacking = true;
            }
            else
            {
                moving = false;
                attacking = false;
            }

        }

        //end turn Button
        buttonRect = new Rect(0, Screen.height - buttonHeight * 1, buttonWidth, buttonHeight);


        if (GUI.Button(buttonRect, "End Turn"))
        {
            actionPoints = 2;
            moving = false;
            attacking = false;

            GameManager.instance.NextTurn();
        }

        base.TurnOnGUI();
    }
}
