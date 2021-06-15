using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using BNG;

public class GameManager : MonoBehaviour, IPunObservable
{
    public int Turn { get { return turn; } }
    public int Moves { get { return moves; } }

    public int MatchStarted { get { return matchStarted; } }

    public UIManager uiManager;

    public Text TextTurn;
    public Text TextMoves;
    public Text TextScore;

    private PhotonView view;

    private List<NetworkedCard> flippedCards;

    private const float SHOWTIME = 2.0f;
    private float showtime = SHOWTIME;
    private bool isShowTime;
    private bool pairFound;

    private int matchStarted;
    private int syncMatchStarted;

    private int turn;
    private int syncTurn;

    private int moves;
    private int syncMoves;

    private int scoreHost;
    private int syncScoreHost;
    private int scoreGuest;
    private int syncScoreGuest;

    void Awake()
    {
        view = GetComponent<PhotonView>();

        flippedCards = new List<NetworkedCard>();

        matchStarted = 0;
        syncMatchStarted = 0;

        turn = 0;
        syncTurn = 0;

        moves = 2;
        syncMoves = 2;

        UpdateTurn(turn);
        UpdateMoves(moves);
        UpdateScore(scoreHost, scoreGuest);
    }

    void Update()
    {
        Sync();

        ShowTime();
    }

    public void UseMove(NetworkedCard newCard)
    {
        if (!view.IsMine) return;

        if (flippedCards.Contains(newCard)) return;

        flippedCards.Add(newCard);

        UpdateMoves(--moves);

        if (moves > 0) return;

        pairFound = (flippedCards[0].Number == flippedCards[1].Number);

        isShowTime = true;
    }

    private void ShowTime()
    {
        if (!isShowTime) return;

        showtime -= Time.deltaTime;

        if (showtime > 0.0f) return;

        isShowTime = false;
        showtime = SHOWTIME;

        if (pairFound)
        {
            if (turn == 0)
                UpdateScore(++scoreHost, scoreGuest);
            else
                UpdateScore(scoreHost, ++scoreGuest);

            flippedCards[0].ResetFlipped();
            flippedCards[1].ResetFlipped();
        }
        else
        {
            flippedCards[0].ResetNotFlipped();
            flippedCards[1].ResetNotFlipped();

            ChangePlayer();
        }

        flippedCards.Clear();
        UpdateMoves(moves = 2);
    }

    public void ChangePlayer()
    {
        UpdateTurn(turn = turn == 0 ? 1 : 0);

        if ((PhotonNetwork.IsMasterClient && turn == 0) || (!PhotonNetwork.IsMasterClient && turn == 1))
            uiManager.PlayTurnAnimation();
    }

    public void StartMatch()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        ResetStats();

        turn = Random.Range(0.0f, 100.0f) >= 50.0f ? 0 : 1;
        ChangePlayer();

        matchStarted = 1;
    }

    private void ResetStats()
    {
        UpdateTurn(turn = 0);
        UpdateMoves(moves = 2);
        UpdateScore(scoreHost = 0, scoreGuest = 0);
    }

    private void Sync()
    {
        if (view.IsMine) return;

        UpdateTurn(turn = syncTurn);
        UpdateMoves(moves = syncMoves);
        UpdateScore(scoreHost = syncScoreHost, scoreGuest = syncScoreGuest);
        matchStarted = syncMatchStarted;
    }

    private void UpdateTurn(int currentTurn)
    {
        if (currentTurn == 0)
            TextTurn.text = "<color=white>TURN</color>\n<color=red>" + currentTurn + "</color>";
        else
            TextTurn.text = "<color=white>TURN</color>\n<color=cyan>" + currentTurn + "</color>";
    }

    private void UpdateMoves(int currentMoves)
    {
        TextMoves.text = "<color=white>MOVES\n" + currentMoves + "</color>";
    }

    private void UpdateScore(int scoreHost, int scoreGuest)
    {
        string currentScore = "<color=white>SCORE</color>\n";
        currentScore += "<color=red> " + scoreHost + "</color>";
        currentScore += "<color=white> - </color>";
        currentScore += "<color=cyan> " + scoreGuest + "</color>";
        TextScore.text = currentScore;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && view.IsMine)
        {
            stream.SendNext(turn);
            stream.SendNext(moves);
            stream.SendNext(scoreHost);
            stream.SendNext(scoreGuest);
            stream.SendNext(matchStarted);
        }
        else
        {
            syncTurn = (int)stream.ReceiveNext();
            syncMoves = (int)stream.ReceiveNext();
            syncScoreHost = (int)stream.ReceiveNext();
            syncScoreGuest = (int)stream.ReceiveNext();
            syncMatchStarted = (int)stream.ReceiveNext();
        }
    }
}
