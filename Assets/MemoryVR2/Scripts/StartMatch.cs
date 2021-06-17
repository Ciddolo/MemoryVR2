using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StartMatch : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void Update()
    {
        transform.Rotate(new Vector3(0.0f, 0.1f, 0.0f));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            gameManager.StartMatch();
    }
}
