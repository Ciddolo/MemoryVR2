using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class UIManager : MonoBehaviour
{
    public GameObject UI;

    private Animation startTurnAnimation;

    private Text textTurn;
    private Text textMoves;
    private Text textScore;

    private GameManager gameManager;
    private TurnManager turnManager;

    private void Start()
    {
        gameManager = GetComponent<GameManager>();
        turnManager = GetComponent<TurnManager>();

        startTurnAnimation = UI.GetComponent<Animation>();

        textTurn = UI.transform.GetChild(0).GetChild(0).GetComponent<Text>();
        textMoves = UI.transform.GetChild(0).GetChild(1).GetComponent<Text>();
        textScore = UI.transform.GetChild(0).GetChild(2).GetComponent<Text>();
    }

    public void UpdateTurn()
    {
        if (!gameManager.MatchStarted)
        {
            textTurn.text = "<color=white>MATCH NOT STARTED</color>";
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            if (turnManager.IsMyTurn)
                textTurn.text = "<color=red>YOUR TURN</color>";
            else
                textTurn.text = "<color=cyan>OPPONENT TURN</color>";
        }
        else
        {
            if (turnManager.IsMyTurn)
                textTurn.text = "<color=cyan>YOUR TURN</color>";
            else
                textTurn.text = "<color=red>OPPONENT TURN</color>";
        }
    }

    public void UpdateMoves(int currentMoves)
    {
        textMoves.text = "<color=white>MOVES: " + currentMoves + "</color>";
    }

    public void UpdateScore(int scoreHost, int scoreGuest)
    {
        string currentScore = "<color=white>SCORE: </color>";
        currentScore += "<color=red> " + scoreHost + "</color>";
        currentScore += "<color=white> - </color>";
        currentScore += "<color=cyan> " + scoreGuest + "</color>";
        textScore.text = currentScore;
    }

    public void PlayTurnAnimation()
    {
        startTurnAnimation.Play();
    }
}
