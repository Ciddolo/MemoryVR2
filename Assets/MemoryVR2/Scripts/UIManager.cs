using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private Animation turn;

    private void Start()
    {
        turn = gameObject.GetComponent<Animation>();
    }

    public void PlayTurnAnimation()
    {
        turn.Play();
    }
}
