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
    private CardManager cardManager;

    private PhotonView view;

    private List<NetworkedCard> flippedCards;

    private bool pairFound;

    private int matchStarted;
    private int syncMatchStarted;

    private int moves;
    private int syncMoves;

    private int scoreHost;
    private int syncScoreHost;
    private int scoreGuest;
    private int syncScoreGuest;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    private void Start()
    {
        turnManager = GetComponent<TurnManager>();
        uiManager = GetComponent<UIManager>();
        soundManager = GetComponent<SoundManager>();
        cardManager = transform.GetChild(0).GetComponent<CardManager>();

        flippedCards = new List<NetworkedCard>();

        matchStarted = 0;
        syncMatchStarted = 0;

        moves = 2;
        syncMoves = 2;

        uiManager.UpdateTurn();
        uiManager.UpdateMoves(moves);
        uiManager.UpdateScore(scoreHost, scoreGuest);
    }

    private void Update()
    {
        Sync();
    }

    public void UseMove(NetworkedCard newCard)
    {
        if (flippedCards.Contains(newCard)) return;

        flippedCards.Add(newCard);

        uiManager.UpdateMoves(--moves);

        soundManager.PlaySound(SoundClip.Beep);

        if (moves <= 0)
            StartCoroutine(CheckPair());
    }

    public IEnumerator CheckPair()
    {
        pairFound = (flippedCards[0].Number == flippedCards[1].Number);

        if (pairFound)
            soundManager.PlaySound(SoundClip.BeepUp);
        else
            soundManager.PlaySound(SoundClip.BeepDown);

        yield return new WaitForSeconds(2);

        if (!pairFound)
            ChangePlayer();
        else
        {
            if (turnManager.IsHostTurn)
                uiManager.UpdateScore(++scoreHost, scoreGuest);
            else if (turnManager.IsGuestTurn)
                uiManager.UpdateScore(scoreHost, ++scoreGuest);

            flippedCards[0].IsDone = true;
            flippedCards[1].IsDone = true;
        }

        for (int i = 0; i < transform.GetChild(0).childCount; i++)
            transform.GetChild(0).GetChild(i).GetComponent<NetworkedCard>().GoToBoard();

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

        cardManager.ShuffleCards();

        soundManager.PlaySound(SoundClip.BeepUp);
    }

    private void ResetStats()
    {
        if (!PhotonNetwork.IsMasterClient) return;

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
