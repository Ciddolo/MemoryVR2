using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using BNG;

public class GameManager : MonoBehaviour, IPunObservable
{
    public bool HaveMoves { get { return moves > 0; } }
    public bool MatchStarted { get { return matchStarted != 0; } }
    public bool IsHostTurn { get { return turn == 0; } }
    public bool IsGuestTurn { get { return turn == 1; } }
    public bool IsMyTurn { get { return PhotonNetwork.IsMasterClient ? IsHostTurn : IsGuestTurn; } }

    //private TurnManager turnManager;
    private UIManager uiManager;
    private SoundManager soundManager;
    private CardManager cardManager;

    private PhotonView view;

    private List<NetworkedCard> flippedCards;

    private const float SHOWTIME = 2.0f;
    private float showtime = SHOWTIME;
    private bool isShowTime;
    private bool pairFound;

    private int matchStarted;
    private int syncMatchStarted;

    private int moves;
    private int syncMoves;

    private int scoreHost;
    private int syncScoreHost;
    private int scoreGuest;
    private int syncScoreGuest;

    private bool changeTurn;
    private int turn;
    private int syncTurn;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    private void Start()
    {
        //turnManager = GetComponent<TurnManager>();
        uiManager = GetComponent<UIManager>();
        soundManager = GetComponent<SoundManager>();
        cardManager = transform.GetChild(0).GetComponent<CardManager>();

        flippedCards = new List<NetworkedCard>();

        matchStarted = 0;
        syncMatchStarted = 0;

        moves = 2;
        syncMoves = 2;

        turn = -5;
        syncTurn = -5;

        uiManager.UpdateTurn();
        uiManager.UpdateMoves(moves);
        uiManager.UpdateScore(scoreHost, scoreGuest);
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && changeTurn)
            ChangePlayer();

        Sync();

        ShowTime();

        uiManager.UpdateDebug(turn.ToString());
    }

    public void UseMove(NetworkedCard newCard)
    {
        if (flippedCards.Contains(newCard)) return;

        flippedCards.Add(newCard);

        uiManager.UpdateMoves(--moves);

        if (moves > 0)
        {
            soundManager.PlaySound(SoundClip.Beep);
            return;
        }

        pairFound = (flippedCards[0].Number == flippedCards[1].Number);

        if (pairFound)
            soundManager.PlaySound(SoundClip.BeepUp);
        else
            soundManager.PlaySound(SoundClip.BeepDown);

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
                uiManager.UpdateScore(++scoreHost, scoreGuest);
            else if (turn == 1)
                uiManager.UpdateScore(scoreHost, ++scoreGuest);

            flippedCards[0].IsDone = true;
            flippedCards[1].IsDone = true;
        }
        else
            ChangePlayer();

        for (int i = 0; i < transform.GetChild(0).childCount; i++)
            transform.GetChild(0).GetChild(i).GetComponent<NetworkedCard>().GoToBoard();

        flippedCards.Clear();
        uiManager.UpdateMoves(moves = 2);
    }

    public void ChangePlayer()
    {
        SetTurn();

        uiManager.UpdateTurn();

        if (IsMyTurn)
        {
            uiManager.PlayTurnAnimation();

            soundManager.PlaySound(SoundClip.SwitchTurn);

            changeTurn = false;
        }
    }

    public void StartMatch()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (MatchStarted) return;

        matchStarted = 1;

        ResetStats();

        cardManager.ShuffleCards();

        soundManager.PlaySound(SoundClip.BeepUp);
    }

    private void ResetStats()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        SetRandomTurn();

        uiManager.UpdateScore(scoreHost = 0, scoreGuest = 0);
        uiManager.UpdateMoves(moves = 2);
        uiManager.UpdateTurn();

        if (IsMyTurn)
            uiManager.PlayTurnAnimation();
    }

    private void Sync()
    {
        if (view.IsMine) return;

        turn = syncTurn;
        uiManager.UpdateTurn();

        uiManager.UpdateMoves(moves = syncMoves);

        uiManager.UpdateScore(scoreHost = syncScoreHost, scoreGuest = syncScoreGuest);

        matchStarted = syncMatchStarted;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && view.IsMine)
        {
            stream.SendNext(moves);
            stream.SendNext(scoreHost);
            stream.SendNext(scoreGuest);
            stream.SendNext(matchStarted);
            stream.SendNext(turn);
        }
        else
        {
            syncMoves = (int)stream.ReceiveNext();
            syncScoreHost = (int)stream.ReceiveNext();
            syncScoreGuest = (int)stream.ReceiveNext();
            syncMatchStarted = (int)stream.ReceiveNext();
            syncTurn = (int)stream.ReceiveNext();
        }
    }

    public void SetRandomTurn()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        turn = Random.Range(0.0f, 100.0f) >= 50.0f ? 0 : 1;
    }

    public void SetTurn(int newTurn = -1)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (newTurn < 0 || newTurn > 1)
            turn = turn == 0 ? 1 : 0;
        else
            turn = newTurn;
    }
}
