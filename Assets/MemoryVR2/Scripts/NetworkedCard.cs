using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BNG;

public class NetworkedCard : MonoBehaviour
{
    // DEFAULT

    public string Number { get { return numberText.text; } set { numberText.text = value; } }

    private PhotonView view;
    private Grabbable grabbable;
    private Text numberText;

    private int isNumberActive;
    private int syncIsNumberActive;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        grabbable = GetComponent<NetworkedGrabbable>();
        numberText = transform.GetChild(0).GetChild(0).GetComponent<Text>();
        numberText.enabled = false;
        isNumberActive = 0;
    }

    private void Update()
    {
        numberText.enabled = Vector3.Dot(transform.up, Vector3.up) >= 0.0f;

        GameplaySync();
    }

    private void GameplaySync()
    {
        if (view.IsMine)
        {
            isNumberActive = numberText.enabled ? 1 : 0;
        }
        else
        {
            numberText.enabled = syncIsNumberActive != 0;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && view.IsMine)
        {
            stream.SendNext(isNumberActive);
        }
        else
        {
            syncIsNumberActive = (int)stream.ReceiveNext();
        }
    }
}
