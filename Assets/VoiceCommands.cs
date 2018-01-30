using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class VoiceCommands : MonoBehaviour {

    KeywordRecognizer keywordRecognizer;

    // Use this for initialization
    void Start () {
        // Setup a keyword recognizer to enable resetting the target location.
        List<string> keywords = new List<string>();
        keywords.Add("Move");
        keywords.Add("Attack");
        keywords.Add("End Turn");
        keywords.Add("Move Map");
        keywords.Add("Resize Map");
        
        keywordRecognizer = new KeywordRecognizer(keywords.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log(args.text);
        switch (args.text)
        {
            case "Move":
                PlayerMove();
                break;
            case "Attack":
                PlayerAttack();
                break;
            case "End Turn":
                EndTurn();
                break;
            case "Move Map":
                HologramPlacement.Instance.ResetStage();
                break;
            case "Resize Map":
                EndTurn();
                break;
        }
    }

    private void PlayerMove()
    {
        Player player = GameManager.instance.players[GameManager.instance.currentPlayerIndex];

        if (!player.moving)
        {
            //GameManager.instance.removeTileHighlights();
            player.moving = true;
            player.attacking = false;
            GameManager.instance.highlightTilesAt(player.gridPosition, Color.blue, player.movementPerActionPoint, false);
        }
        else
        {
            player.moving = false;
            player.attacking = false;
            GameManager.instance.removeTileHighlights();
        }
    }

    private void PlayerAttack()
    {
        Player player = GameManager.instance.players[GameManager.instance.currentPlayerIndex];

        if (!player.attacking)
        {
            GameManager.instance.removeTileHighlights();
            player.moving = false;
            player.attacking = true;
            GameManager.instance.highlightTilesAt(player.gridPosition, Color.red, player.attackRange);
        }
        else
        {
            player.moving = false;
            player.attacking = false;
            GameManager.instance.removeTileHighlights();
        }
    }

    private void EndTurn()
    {
        Player player = GameManager.instance.players[GameManager.instance.currentPlayerIndex];

        GameManager.instance.removeTileHighlights();
        player.actionPoints = 2;
        player.moving = false;
        player.attacking = false;
        GameManager.instance.nextTurn();
    }

    private void ResizeMap()
    {
        
    }
}
