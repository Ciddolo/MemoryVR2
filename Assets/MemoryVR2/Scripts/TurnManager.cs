using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TurnManager : MonoBehaviour
{
    public bool IsHostTurn { get { return turn == 0; } }
    public bool IsGuestTurn { get { return turn == 1; } }
    public bool IsMyTurn { get { return PhotonNetwork.IsMasterClient ? IsHostTurn : IsGuestTurn; } }

    private PhotonView view;

    private GameManager gameManager;

    private int turn;
    private int syncTurn;

    private void Awake()
    {
        view = GetComponent<PhotonView>();

        gameManager = GetComponent<GameManager>();

        turn = 0;
        syncTurn = 0;
    }

    private void Update()
    {
        if (!view.IsMine)
            turn = syncTurn;
    }

    public void SetRandomTurn()
    {
        turn = Random.Range(0.0f, 100.0f) >= 50.0f ? 0 : 1;
    }

    public void SetTurn(int newTurn = -1)
    {
        if (newTurn < 0 || newTurn > 1)
            turn = turn == 0 ? 1 : 0;
        else
            turn = newTurn;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && view.IsMine)
        {
            stream.SendNext(turn);
        }
        else
        {
            syncTurn = (int)stream.ReceiveNext();
        }
    }
}
