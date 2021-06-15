using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StartMatch : MonoBehaviour
{
    GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (gameManager.MatchStarted != 0) return;

        if (other.tag == "Player")
            gameManager.StartMatch();
    }
}
