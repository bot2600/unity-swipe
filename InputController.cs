using UnityEngine;

[RequireComponent(typeof(SwipeManager))]
public class InputController : MonoBehaviour
{
    //public Player OurPlayer; // Perhaps your playerscript?

    void Start()
    {
        SwipeManager swipeManager = GetComponent<SwipeManager>();
        SwipeManager.OnSwipeDetected += HandleSwipe;
    }

    void HandleSwipe(Swipe swipe, Vector2 swipeVelocity)
    {
        if (swipe == Swipe.Up)
        {
            Debug.logger.Log("Swipe Up Detected");
        }
        else if (swipe == Swipe.Right)
        {
            Debug.logger.Log("Swipe Right Detected");
        }
        else if (swipe == Swipe.Down)
        {
            Debug.logger.Log("Swipe Down Detected");
        }
        else if (swipe == Swipe.Left)
        {
            Debug.logger.Log("Swipe Left Detected");
        }
    }
}