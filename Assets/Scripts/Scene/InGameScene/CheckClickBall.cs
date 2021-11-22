using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckClickBall : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball == null)
            return;

        if (ball.IsLocalBall)
            ball.MyMaterial.SetColor("_OutlineColor", Color.red);

        CatchBall.Instance.UnAvailableList.Add(ball);
    }

    private void OnTriggerExit(Collider other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball == null)
            return;

        if (ball.IsLocalBall)
            ball.MyMaterial.SetColor("_OutlineColor", Color.white);

        CatchBall.Instance.UnAvailableList.RemoveAll(x => x == ball);
    }
}

