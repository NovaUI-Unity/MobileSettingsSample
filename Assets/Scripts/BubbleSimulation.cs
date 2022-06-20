using Nova;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// Responsible for running a bouncing bubble collision simulation using the Nova Animation system and <see cref="Interaction.SphereCollide(Sphere, out UIBlockHit, int)"/>.
    /// </summary>
    public class BubbleSimulation : MonoBehaviour
    {
        [Tooltip("The parent of all the bubble objects to animate in the simulation.")]
        public UIBlock BubbleRoot = null;

        [Tooltip("The layer mask of the collidable bubbles.")]
        public LayerMask BubbleCollisionMask;

        [Tooltip("The velocity of the bubbles in local space.")]
        public float BubbleVelocity = 50;

        /// <summary>
        /// The handle tracking all of the bubble animations.
        /// </summary>
        private AnimationHandle bubblesAnimation;

        private void OnEnable()
        {
            // Add a click gesture handler to make the bubbles playfully interactive
            BubbleRoot.AddGestureHandler<Gesture.OnClick>(HandleBubbleClicked);

            // Initialize the simulation by kicking off all the bubble animations
            InitializeBubblesSimulation();
        }

        private void OnDisable()
        {
            // Remove the bubble gesture handler
            BubbleRoot.RemoveGestureHandler<Gesture.OnClick>(HandleBubbleClicked);

            // Cancel the running simulation
            bubblesAnimation.Cancel();
        }

        /// <summary>
        /// Kick off all the animations which make up this "simulation"
        /// </summary>
        private void InitializeBubblesSimulation()
        {
            // Ensure the bubble root's layout properties are calculated, since they'll be read in the animation.
            BubbleRoot.CalculateLayout();
            
            // An arbitrary "distance" used to convert the bubble velocity
            // into a loop duration. This is the distance each bubble will move
            // per animation iteration.
            const float distanceToMove = 1000;
            
            // Velocity = Distance / Time => Time = Distance / Velocity
            float simulationLoopDuration = distanceToMove / BubbleVelocity;

            for (int i = 0; i < BubbleRoot.ChildCount; ++i)
            {
                // Create a new animation per child of the BubbleRoot
                BubbleAnimation bubbleAnimation = new BubbleAnimation()
                {
                    UIBlock = BubbleRoot.GetChild(i) as UIBlock2D,
                    DistanceToMove = distanceToMove,
                    CollisionMask = BubbleCollisionMask,
                };

                // Group all animations together, so we can track the simution with a single
                // AnimationHandle.
                bubblesAnimation = bubblesAnimation.Include(bubbleAnimation, simulationLoopDuration, AnimationHandle.Infinite);
            }
        }

        /// <summary>
        /// Update the size of a bubble each time it's clicked
        /// </summary>
        /// <param name="evt">The click event data.</param>
        private void HandleBubbleClicked(Gesture.OnClick evt)
        {
            // The receiver is the clicked object,
            // which will be one of the bubbles.
            UIBlock bubble = evt.Receiver;

            // Generate a new random size for the bubble.
            bubble.Size.XY = Length.FixedValue(Random.Range(50f, 200f));
        }
    }
}
