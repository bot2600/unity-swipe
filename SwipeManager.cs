using UnityEngine;
using System.Collections.Generic;
using System.Linq;

class CardinalDirection
{
    public static readonly Vector2 Up = new Vector2(0, 1);
    public static readonly Vector2 Down = new Vector2(0, -1);
    public static readonly Vector2 Right = new Vector2(1, 0);
    public static readonly Vector2 Left = new Vector2(-1, 0);
    public static readonly Vector2 UpRight = new Vector2(1, 1);
    public static readonly Vector2 UpLeft = new Vector2(-1, 1);
    public static readonly Vector2 DownRight = new Vector2(1, -1);
    public static readonly Vector2 DownLeft = new Vector2(-1, -1);
}

public enum Swipe { None, Up, Down, Left, Right, UpLeft, UpRight, DownLeft, DownRight };

public class SwipeManager : MonoBehaviour
{
    [Tooltip("Min swipe distance (inches) to register as swipe")]
    [SerializeField]
    float minSwipeLength = 0.5f;

    [Tooltip("If true, a swipe is counted when the min swipe length is reached. If false, a swipe is counted when the touch/click ends.")]
    [SerializeField]
    bool triggerSwipeAtMinLength = false;

    [Tooltip("Whether to detect eight or four cardinal directions")]
    [SerializeField]
    bool useEightDirections = false;

    const float eightDirAngle = 0.906f;
    const float fourDirAngle = 0.5f;
    const float defaultDPI = 72f;
    const float dpcmFactor = 2.54f;

    static Dictionary<Swipe, Vector2> cardinalDirections = new Dictionary<Swipe, Vector2>()
    {
        { Swipe.Up, CardinalDirection.Up },
        { Swipe.Down, CardinalDirection.Down },
        { Swipe.Right, CardinalDirection.Right },
        { Swipe.Left, CardinalDirection.Left },
        { Swipe.UpRight, CardinalDirection.UpRight },
        { Swipe.UpLeft, CardinalDirection.UpLeft },
        { Swipe.DownRight, CardinalDirection.DownRight },
        { Swipe.DownLeft, CardinalDirection.DownLeft }
    };

    public delegate void OnSwipeDetectedHandler(Swipe swipeDirection, Vector2 swipeVelocity);

    static OnSwipeDetectedHandler _OnSwipeDetected;

    public static event OnSwipeDetectedHandler OnSwipeDetected
    {
        add
        {
            _OnSwipeDetected += value;
            autoDetectSwipes = true;
        }
        remove
        {
            _OnSwipeDetected -= value;
        }
    }

    public static Vector2 swipeVelocity;

    static float dpcm;
    static float swipeStartTime;
    static float swipeEndTime;
    static bool autoDetectSwipes;
    static bool swipeEnded;
    static Swipe swipeDirection;
    static Vector2 firstPressPos;
    static Vector2 secondPressPos;
    static SwipeManager instance;

    void Awake()
    {
        instance = this;
        float dpi = (Screen.dpi == 0) ? defaultDPI : Screen.dpi;
        dpcm = dpi / dpcmFactor;
    }

    void Update()
    {
        if (autoDetectSwipes) { DetectSwipe(); }
    }

    /// <summary>
    /// Attempts to detect the current swipe direction.
    /// Should be called over multiple frames in an Update-like loop.
    /// </summary>
    static void DetectSwipe()
    {
        if (!GetTouchInput() && !GetMouseInput())
        {
            swipeDirection = Swipe.None;
            return;
        }

        if (swipeEnded) { return; }

        Vector2 currentSwipe = secondPressPos - firstPressPos;
        float swipeCm = currentSwipe.magnitude / dpcm;

        if (swipeCm < instance.minSwipeLength)
        {
            if (!instance.triggerSwipeAtMinLength) { swipeDirection = Swipe.None; }
            return;
        }

        swipeEndTime = Time.time;
        swipeVelocity = currentSwipe * (swipeEndTime - swipeStartTime);
        swipeDirection = GetSwipeDirectionByTouch(currentSwipe);
        swipeEnded = true;

        if (_OnSwipeDetected != null) { _OnSwipeDetected(swipeDirection, swipeVelocity); }
    }

    static bool GetTouchInput()
    {
        if (Input.touches.Length == 0) { return false; }

        Touch t = Input.GetTouch(0);
        if (t.phase == TouchPhase.Began)
        {
            firstPressPos = t.position;
            swipeStartTime = Time.time;
            swipeEnded = false;
            return false;
        }
        if (t.phase == TouchPhase.Ended)
        {
            secondPressPos = t.position;
            return true;
        }

        return instance.triggerSwipeAtMinLength;
    }

    static bool GetMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            firstPressPos = Input.mousePosition;
            swipeStartTime = Time.time;
            swipeEnded = false;
            return false;
        }
        if (Input.GetMouseButtonUp(0))
        {
            secondPressPos = Input.mousePosition;
            return true;
        }
        return instance.triggerSwipeAtMinLength;
    }

    static Swipe GetSwipeDirectionByTouch(Vector2 currentSwipe)
    {
        currentSwipe.Normalize();
        return cardinalDirections.FirstOrDefault(direction => IsDirection(currentSwipe, direction.Value)).Key;
    }

    static bool IsDirection(Vector2 direction, Vector2 cardinalDirection)
    {
        return Vector2.Dot(direction, cardinalDirection) > (instance.useEightDirections ? eightDirAngle : fourDirAngle);
    }
}