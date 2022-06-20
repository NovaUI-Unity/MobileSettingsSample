using Nova;
using UnityEngine;

public class MouseInputManager : MonoBehaviour
{
    public const uint MousePointerControlID = 1;
    public const uint ScrollWheelControlID = 2;

    /// <summary>
    /// Inverts the mouse wheel scroll direction.
    /// </summary>
    public bool InvertScrolling = true;

    private void Update()
    {
        if (!Input.mousePresent)
        {
            // Nothing to do, no mouse device detected
            return;
        }

        // Get the current world-space ray of the mouse
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Get the current scroll wheel delta
        Vector2 mouseScrollDelta = Input.mouseScrollDelta;

        // Check if there is any scrolling this frame
        if (mouseScrollDelta != Vector2.zero)
        {
            // Invert scrolling for a mouse-type experience,
            // otherwise will scroll track-pad style.
            if (InvertScrolling)
            {
                mouseScrollDelta.y *= -1f;
            }

            // Create a new Interaction.Update from the mouse ray and scroll wheel control id
            Interaction.Update scrollInteraction = new Interaction.Update(mouseRay, ScrollWheelControlID);

            // Feed the scroll update and scroll delta into Nova's Interaction APIs
            Interaction.Scroll(scrollInteraction, mouseScrollDelta);
        }

        bool leftMouseButtonDown = Input.GetMouseButton(0);

        // Create a new Interaction.Update from the mouse ray and pointer control id
        Interaction.Update pointInteraction = new Interaction.Update(mouseRay, MousePointerControlID);

        // Feed the pointer update and pressed state to Nova's Interaction APIs
        Interaction.Point(pointInteraction, leftMouseButtonDown);
    }
}
