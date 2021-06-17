using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BNG;

public class NetworkedCard : MonoBehaviour
{
    public string Number { get { return numberText.text; } set { numberText.text = value; } }
    public bool IsDone { get { return isDone; } set { isDone = value; } }

    public Material FaceDefaultMaterial;
    public Material FaceMaterial;

    private GameManager gameManager;
    private PhotonView view;
    private NetworkedGrabbable grabbable;
    private Text numberText;
    private MeshRenderer bodyRenderer;
    private MeshRenderer faceRenderer;

    private bool isDone;
    private bool isFlipped;

    private Vector3 defaultPosition;
    private Vector3 defaultRotation;

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        view = GetComponent<PhotonView>();
        grabbable = GetComponent<NetworkedGrabbable>();

        numberText = transform.GetChild(0).GetChild(0).GetComponent<Text>();
        numberText.enabled = false;

        bodyRenderer = transform.GetChild(1).GetComponent<MeshRenderer>();
        faceRenderer = transform.GetChild(2).GetComponent<MeshRenderer>();

        defaultPosition = transform.localPosition;
        defaultRotation = transform.localRotation.eulerAngles;
    }

    private void Update()
    {
        CheckIsFlipped();
    }

    private void CheckIsFlipped()
    {
        if (isFlipped) return;

        isFlipped = Vector3.Dot(transform.up, -Vector3.forward) >= 0.0f;

        if (!isFlipped) return;

        SetFaceMaterial(FaceMaterial);
        numberText.enabled = true;
        gameManager.UseMove(this);
    }

    public void SetBodyMaterial(Material newMaterial)
    {
        if (bodyRenderer)
            bodyRenderer.GetComponent<MeshRenderer>().material = newMaterial;
        else
            transform.GetChild(1).GetComponent<MeshRenderer>().material = newMaterial;
    }

    public void SetFaceMaterial(Material newMaterial)
    {
        if (faceRenderer)
            faceRenderer.GetComponent<MeshRenderer>().material = newMaterial;
        else
            transform.GetChild(2).GetComponent<MeshRenderer>().material = newMaterial;
    }

    public void ResetNotFlipped()
    {
        grabbable.DropItem(grabbable.GetPrimaryGrabber());

        transform.localPosition = defaultPosition;
        transform.rotation = Quaternion.Euler(defaultRotation);

        isFlipped = false;

        SetFaceMaterial(FaceDefaultMaterial);
        numberText.enabled = false;
    }

    public void ResetFlipped()
    {
        grabbable.DropItem(grabbable.GetPrimaryGrabber());

        transform.localPosition = defaultPosition;
        transform.rotation = Quaternion.Euler(new Vector3(defaultRotation.x, defaultRotation.y, 0.0f));

        isFlipped = true;
        isDone = true;

        SetFaceMaterial(FaceMaterial);
        numberText.enabled = true;
    }
}
