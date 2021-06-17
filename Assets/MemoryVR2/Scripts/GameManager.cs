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

    private TurnManager turnManager;
    private UIManager uiManager;
    private SoundManager soundManager;

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

    void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    private void Start()
    {
        turnManager = GetComponent<TurnManager>();
        uiManager = GetComponent<UIManager>();
        soundManager = GetComponent<SoundManager>();

        flippedCards = new List<NetworkedCard>();

        matchStarted = 0;
        syncMatchStarted = 0;

        moves = 2;
        syncMoves = 2;

        uiManager.UpdateTurn();
        uiManager.UpdateMoves(moves);
        uiManager.UpdateScore(scoreHost, scoreGuest);
    }

    void Update()
    {
        Sync();

        ShowTime();
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
            if (turnManager.IsHostTurn)
                uiManager.UpdateScore(++scoreHost, scoreGuest);
            else if (turnManager.IsGuestTurn)
                uiManager.UpdateScore(scoreHost, ++scoreGuest);

            flippedCards[0].IsDone = true;
            flippedCards[1].IsDone = true;
        }
        else
            ChangePlayer();

        Transform cardsParent = transform.GetChild(0);
        for (int i = 0; i < cardsParent.childCount; i++)
        {
            NetworkedCard currentCard = cardsParent.GetChild(i).GetComponent<NetworkedCard>();
            currentCard.GoToBoard();
        }

        flippedCards.Clear();
        uiManager.UpdateMoves(moves = 2);
    }

    public void ChangePlayer()
    {
        turnManager.SetTurn();

        uiManager.UpdateTurn();

        if (turnManager.IsMyTurn)
        {
            uiManager.PlayTurnAnimation();

            soundManager.PlaySound(SoundClip.SwitchTurn);
        }
    }

    public void StartMatch()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (MatchStarted) return;

        matchStarted = 1;

        ResetStats();

        soundManager.PlaySound(SoundClip.BeepUp);
    }

    private void ResetStats()
    {
        turnManager.SetRandomTurn();

        uiManager.UpdateScore(scoreHost = 0, scoreGuest = 0);
        uiManager.UpdateMoves(moves = 2);
        uiManager.UpdateTurn();

        if (turnManager.IsMyTurn)
            uiManager.PlayTurnAnimation();
    }

    private void Sync()
    {
        if (view.IsMine) return;

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
        }
        else
        {
            syncMoves = (int)stream.ReceiveNext();
            syncScoreHost = (int)stream.ReceiveNext();
            syncScoreGuest = (int)stream.ReceiveNext();
            syncMatchStarted = (int)stream.ReceiveNext();
        }
    }
}
