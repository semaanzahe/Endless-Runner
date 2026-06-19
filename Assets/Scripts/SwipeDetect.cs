using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class SwipeDetect : MonoBehaviour
{
    
    private Vector2 prevTouchPos;
    bool startSwipeDetect = false;
    private void Update()
    {
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began && !startSwipeDetect)
            {
                startSwipeDetect = true; 
                prevTouchPos = touch.screenPosition;
            }
            
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended && startSwipeDetect)
            {
                startSwipeDetect = false;
                Vector2 swipeDiraction =  touch.screenPosition - prevTouchPos;
                if (swipeDiraction.magnitude > 50)
                {
                    swipeDiraction.Normalize();
                    Debug.Log($"Swipe Detected! direction: {swipeDiraction}");
                }
            }
        } 
        
        
    }
}
