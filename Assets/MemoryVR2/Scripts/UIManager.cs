using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Transform PlayerHead;
    public float Offset = 1.0f;

    void Update()
    {
        transform.position = PlayerHead.position + (PlayerHead.forward * Offset);
        transform.LookAt(PlayerHead, Vector3.up);
    }
}
