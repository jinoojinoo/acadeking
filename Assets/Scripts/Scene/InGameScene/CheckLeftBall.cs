using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckLeftBall : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball == null)
            return;
        if (ball.IsLocalBall == false)
            return;

        CatchBall.Instance.LeftBallList.Add(ball);
    }

    private void OnTriggerExit(Collider other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball == null)
            return;
        if (ball.IsLocalBall == false)
            return;

        CatchBall.Instance.LeftBallList.RemoveAll(x => x == ball);
    }
}
