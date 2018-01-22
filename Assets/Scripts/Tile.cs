using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

    public Vector2 gridPosition = Vector2.zero;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnMouseEnter()
    {
        if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].moving)
        {
            transform.GetComponent<Renderer>().material.color = Color.blue;
            //Debug.Log("meine Position ist " + gridPosition.x + " | " + gridPosition.y);
        }
        else if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].attacking)
        {

            transform.GetComponent<Renderer>().material.color = Color.red;
            //Debug.Log("meine Position ist " + gridPosition.x + " | " + gridPosition.y);
        }
        
    }

    void OnMouseExit()
    {
        transform.GetComponent<Renderer>().material.color = Color.white;
    }

    void OnMouseDown()
    {
        if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].moving)
        {
            GameManager.instance.MoveCurrentPlayer(this);
        }
        else if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].attacking)
        {
            GameManager.instance.AttackWithCurrentPlayer(this);
        }

        
    }
}
