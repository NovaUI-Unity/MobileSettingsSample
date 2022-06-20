using Nova;
using UnityEngine;

public class TouchInputManager : MonoBehaviour
{
    private void Update()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            // Using Unity's legacy input system, get the first touch point.
            Touch touch = Input.GetTouch(i);

            // Convert the touch point to a world-space ray.
            Ray ray = Camera.main.ScreenPointToRay(touch.position);

            // Create a new Interaction from the ray and the finger's ID
            Interaction.Update interaction = new Interaction.Update(ray, (uint)touch.fingerId);

            // Get the current touch phase
            TouchPhase touchPhase = touch.phase;

            // If the touch phase hasn't ended and hasn't been canceled, then pointerDown == true.
            bool pointerDown = touchPhase != TouchPhase.Canceled && touchPhase != TouchPhase.Ended;

            // Feed the update and pressed state to Nova's Interaction APIs
            Interaction.Point(interaction, pointerDown);
        }
    }
}