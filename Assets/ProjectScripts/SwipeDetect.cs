using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class SwipeDetect : MonoBehaviour
{
    
    private Vector2 prevTouchPos;
    bool startSwipeDetect = false;

    public PlayerMovement Player;
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
                
                if (swipeDiraction.x > 0)
                {
                    swipeDiraction.Normalize();
                    Debug.Log($"Swipe Detected! direction: {swipeDiraction.magnitude}");
                    Player.MoveRight();
                }
                else if(swipeDiraction.x < 0)
                {
                    swipeDiraction.Normalize();
                    Debug.Log($"Swipe Detected! direction: {swipeDiraction}");
                    Player.MoveLeft();
                }
                if (swipeDiraction.y > 0.74)
                {
                    Player.Jump();
                }
                
            }
        } 
        
        
    }
}
