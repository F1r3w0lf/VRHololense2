using System;
using System.Collections;
using System.Collections.Generic;
using Academy.HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.XR.WSA.Input;

public class VoiceCommands : MonoBehaviour {

    KeywordRecognizer keywordRecognizer;
    private GestureRecognizer gestureRecognizer;

    private Vector3 startPos;
    private bool transformationActive = false;

    enum CommandEnum
    {
        Move = 0,
        Attack,
        EndTurn,
        MoveMap,
        ResizeMapSmaller,
        ResizeMapBigger,
        ResizeFinished,
        RotateMapLeft,
        RotateMapRight,
        RotateFinished
    }

    private CommandEnum CurrentCommand { get; set; }

    // Use this for initialization
    void Start () {
        // Setup a keyword recognizer to enable resetting the target location.
        List<string> keywords = new List<string>();
        keywords.Add("Move");
        keywords.Add("Attack");
        keywords.Add("End Turn");
        keywords.Add("Move Map");
        keywords.Add("Resize Map Smaller");
        keywords.Add("Resize Map Bigger");
        keywords.Add("Resize Finished");
        keywords.Add("Rotate Map Left");
        keywords.Add("Rotate Map Right");
        keywords.Add("Rotate Finished");

        keywordRecognizer = new KeywordRecognizer(keywords.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();

        gestureRecognizer = new GestureRecognizer();
        gestureRecognizer.SetRecognizableGestures(GestureSettings.ManipulationTranslate);

        gestureRecognizer.ManipulationStarted += GestureRecognizer_ManipulationStarted;
        gestureRecognizer.ManipulationUpdated += GestureRecognizer_ManipulationUpdated;
        gestureRecognizer.ManipulationCompleted += GestureRecognizer_ManipulationCompleted;
        gestureRecognizer.ManipulationCanceled += GestureRecognizer_ManipulationCanceled;
    }

    void Update()
    {
        if (transformationActive)
        {
            switch (CurrentCommand)
            {
                case CommandEnum.ResizeMapBigger:
                    HologramPlacement.Instance.ResetStage();
                    gameObject.transform.localScale = gameObject.transform.localScale + Vector3.one * 0.02f * Time.deltaTime;
                    Debug.Log("Manipulation updated.\n" + "localScale: " + transform.localScale);
                    //gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + transform.localScale.y, gameObject.transform.position.z);

                    break;
                case CommandEnum.ResizeMapSmaller:
                    HologramPlacement.Instance.ResetStage();
                    gameObject.transform.localScale = gameObject.transform.localScale + Vector3.one * -0.02f * Time.deltaTime;
                    Debug.Log("Manipulation updated.\n" + "localScale: " + transform.localScale);
                    //gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + transform.localScale.y, gameObject.transform.position.z);

                    break;

                case CommandEnum.RotateMapRight:
                    gameObject.transform.GetChild(1).Rotate(0f, 20f * Time.deltaTime, 0f);
                    Debug.Log("Manipulation updated.\n" + "rotation: " + transform.rotation);
                    break;
                case CommandEnum.RotateMapLeft:
                    gameObject.transform.GetChild(1).Rotate(0f, -20f * Time.deltaTime, 0f);
                    Debug.Log("Manipulation updated.\n" + "rotation: " + transform.rotation);
                    break;
            }
        }
    }

    private void GestureRecognizer_ManipulationCanceled(ManipulationCanceledEventArgs obj)
    {
        Debug.Log("Manipulation canceled.");
    }

    private void GestureRecognizer_ManipulationCompleted(ManipulationCompletedEventArgs obj)
    {
        Debug.Log("Manipulation completed.");
        transformationActive = false;
    }

    private void GestureRecognizer_ManipulationUpdated(ManipulationUpdatedEventArgs obj)
    {
        //float time = Time.deltaTime;
        //switch (CurrentCommand)
        //{
        //    case CommandEnum.ResizeMap:
        //        gameObject.transform.localScale = gameObject.transform.localScale + new Vector3(0.02f * time, 0.02f * time, 0.02f * time);
        //        Debug.Log("Manipulation updated.\n" + "localScale: " + transform.localScale);
        //        gameObject.transform.position = startPos;
        //        break;

        //    case CommandEnum.RotateMap:
        //        gameObject.transform.Rotate(Vector3.up, 10f * time);
        //        Debug.Log("Manipulation updated.\n" + "rotation: " + transform.rotation);
        //        break;

        //}
    }

    private void GestureRecognizer_ManipulationStarted(ManipulationStartedEventArgs obj)
    {
        Debug.Log("Manipulation started.");
        startPos = CursorManager.Instance.transform.position;
        transformationActive = true;
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log(args.text);
        startPos = transform.position;
        switch (args.text)
        {
            case "Move":
                PlayerMove();
                CurrentCommand = CommandEnum.Move;
                break;
            case "Attack":
                PlayerAttack();
                CurrentCommand = CommandEnum.Attack;
                break;
            case "End Turn":
                EndTurn();
                CurrentCommand = CommandEnum.EndTurn;
                break;
            case "Move Map":
                HologramPlacement.Instance.ResetStage();
                CurrentCommand = CommandEnum.MoveMap;
                break;
            case "Resize Map Smaller":
                if (!gestureRecognizer.IsCapturingGestures())
                {
                    gestureRecognizer.StartCapturingGestures();
                }
                CurrentCommand = CommandEnum.ResizeMapSmaller;
                break;
            case "Resize Map Bigger":
                if (!gestureRecognizer.IsCapturingGestures())
                {
                    gestureRecognizer.StartCapturingGestures();
                }
                CurrentCommand = CommandEnum.ResizeMapBigger;
                break;
            case "Resize Finished":
                if (gestureRecognizer.IsCapturingGestures())
                {
                    gestureRecognizer.StopCapturingGestures();
                }
                CurrentCommand = CommandEnum.ResizeFinished;
                break;
            case "Rotate Map Left":
                if (!gestureRecognizer.IsCapturingGestures())
                {
                    gestureRecognizer.StartCapturingGestures();
                }
                CurrentCommand = CommandEnum.RotateMapLeft;
                break;
            case "Rotate Map Right":
                if (!gestureRecognizer.IsCapturingGestures())
                {
                    gestureRecognizer.StartCapturingGestures();
                }
                CurrentCommand = CommandEnum.RotateMapRight;
                break;
            case "Rotate Finished":
                if (gestureRecognizer.IsCapturingGestures())
                {
                    gestureRecognizer.StopCapturingGestures();
                }
                CurrentCommand = CommandEnum.RotateFinished;
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
