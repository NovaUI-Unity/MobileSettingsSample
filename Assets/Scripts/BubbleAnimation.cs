using Nova;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// An animation which will lerp a variety of properties on a target <see cref="UIBlock"/>.
    /// It will also move the target <see cref="UIBlock"/> and test if it intersects other
    /// UIBlocks on a layer included in the <see cref="CollisionMask"/>. If a collision is detected,
    /// the target <see cref="UIBlock"/> will "bounce" and start moving in the reflected direction.
    /// </summary>
    [System.Serializable]
    public struct BubbleAnimation : IAnimation
    {
        [Tooltip("The UIBlock representing a \"bubble\" to animate.")]
        public UIBlock2D UIBlock;

        [Tooltip("The distance to move the UIBlock per animation iteration.")]
        public float DistanceToMove;
        
        [Tooltip("The Layer Mask of the objects to test collisions against.")]
        public LayerMask CollisionMask;

        /// <summary>
        /// The color to end at when the animation iteration completes
        /// </summary>
        private Color targetColor;

        /// <summary>
        /// The color to start from per animation iteration
        /// </summary>
        private Color startColor;

        /// <summary>
        /// The position to end at when the animation iteration completes
        /// </summary>
        private Vector3 targetPosition;

        /// <summary>
        /// The position to start from when animating
        /// </summary>
        private Vector3 startPosition;

        /// <summary>
        /// The end rotation for the animation iteration
        /// </summary>
        private Quaternion targetRotation;

        /// <summary>
        /// The start rotation for the animation iteration
        /// </summary>
        private Quaternion startRotation;

        /// <summary>
        /// The latest "time" within the current animation iteration where a bounce occurred
        /// </summary>
        private float bouncedAtPercent;

        /// <summary>
        /// Step the bubble animation
        /// </summary>
        /// <param name="percentDone">The percent of the animation that has completed in the current iteration.</param>
        public void Update(float percentDone)
        {
            if (percentDone == 0)
            {
                Initialize();
                return;
            }

            // Check for collisions between other bubbles and the parent bounds
            DetectCollisions(percentDone);

            // Lerp the gradient color
            UIBlock.Gradient.Color = Color.Lerp(startColor, targetColor, percentDone);

            // Lerp the rotation to give a slow spin effect
            UIBlock.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, percentDone);

            // Ensure alignment is centered
            UIBlock.Alignment = Alignment.Center;

            // Because we change startPosition and targetPosition per bounce, we need
            // to adjust the lerp percent to maintain a constant movement speed.
            float adjustedPercent = Mathf.Clamp01((percentDone - bouncedAtPercent) / (1 - bouncedAtPercent));

            // Lerp from startPosition (often the bounce origin) to targetPosition
            UIBlock.Position.Value = Vector3.Lerp(startPosition, targetPosition, adjustedPercent);
        }

        /// <summary>
        /// Check if the <see cref="UIBlock"/> collides/intersects with any other bubbles or an interior edge of its parent.
        /// </summary>
        /// <param name="percentDone">The percent of the animation that has completed in the current iteration.</param>
        private void DetectCollisions(float percentDone)
        {
            if (!UIBlock.gameObject.activeInHierarchy)
            {
                // If the UIBlock is inactive, then
                // we don't need to do anything.
                return;
            }

            // First check if the bubble collides with an inner edge of its parent
            bool collidesWithParent = CollidesWithParent(out UIBlockHit hitBubble);
            
            // Default to "not colliding with another bubble"
            bool collidesWithBubble = false;

            // Get the radius of the UIBlock bubble in local space.
            // Making the assumption the UIBlock is a circle,
            // so we only need to check one axis.
            float bubbleRadiusLocalSpace = UIBlock.CalculatedSize.Value.x * 0.5f;

            if (!collidesWithParent)
            {
                // Convert the radius into world space by applying the lossyScale - assuming uniform scale here.
                float bubbleRadiusWorldSpace = bubbleRadiusLocalSpace * UIBlock.transform.lossyScale.x;
                
                // Create a sphere with the UIBlock's position and world radius.
                Sphere sphere = new Sphere(UIBlock.transform.position, bubbleRadiusWorldSpace);

                // Cache the UIBlock layer, so we can restore it later
                int actualLayer = UIBlock.gameObject.layer;

                // Move the UIBlock to the default layer to ensure it's not on the layer 
                // we want to test against (assuming we're not testing against the default layer).
                // Otherwise the sphere collision test will always collide with this UIBlock,
                // which isn't what we want to check.
                UIBlock.GameObjectLayer = 0;

                // Perform the collision test, filtered to objects on the layers provided by the CollisionMask.
                // This collision test will return true if the given sphere intersects the rectangular bounds
                // of a UIBlock on the targeted layer.
                if (Interaction.SphereCollide(sphere, out hitBubble, CollisionMask))
                {
                    // Making the assumption the hit UIBlock is a circle, just like the UIBlock 
                    // we're colliding from, since we're targeting objects on a specific layer,
                    // so we only need to check one axis.
                    float hitBubbleRadiusWorldSpace = hitBubble.UIBlock.CalculatedSize.X.Value * hitBubble.UIBlock.transform.lossyScale.x * 0.5f;
                    
                    // The translation in worldspace from the hit bubble, to our local bubble.
                    Vector3 hitToUIBlock = UIBlock.transform.position - hitBubble.UIBlock.transform.position;

                    // If the distance between the UIBlocks' center positions is less than
                    // the sum of their radii, then the UIBlocks collide 
                    float worldSpaceMinDistance = bubbleRadiusWorldSpace + hitBubbleRadiusWorldSpace;
                    
                    // Squared to avoid needing a square root calculation below
                    float worldSpaceMinDistanceSq = worldSpaceMinDistance * worldSpaceMinDistance;

                    if (hitToUIBlock.sqrMagnitude <= worldSpaceMinDistanceSq)
                    {
                        // Indicate the collision status
                        collidesWithBubble = true;

                        // The normal direction of the collision
                        hitBubble.Normal = hitToUIBlock.normalized;
                        
                        // The collision point on the circular bounds of the hit UIBlock
                        hitBubble.Position = hitBubble.UIBlock.transform.position + hitBubble.Normal * hitBubbleRadiusWorldSpace;
                    }
                }

                // Restore the UIBlock's layer
                UIBlock.GameObjectLayer = actualLayer;
            }

            if (!collidesWithParent && !collidesWithBubble)
            {
                // No collision detected, nothing to update.
                return;
            }

            // Convert the collision normal into parent local space
            Vector3 collisionNormalLocalSpace = UIBlock.transform.parent.InverseTransformDirection(hitBubble.Normal);

            // Get the UIBlock's current movement direction vector
            Vector3 movementDirectionLocalSpace = Vector3.Normalize(targetPosition - startPosition);
            
            // Calculate the bounce direction vector
            Vector3 bounceDirectionLocalSpace = Vector3.Reflect(movementDirectionLocalSpace, collisionNormalLocalSpace);

            // Convert the point of collision into local space
            Vector3 collisionPointLocalSpace = UIBlock.transform.parent.InverseTransformPoint(hitBubble.Position);

            // Adjust start position to ensure we aren't starting from an overlapping point (that's what +1 is handling)
            startPosition = collisionPointLocalSpace + collisionNormalLocalSpace * (bubbleRadiusLocalSpace + 1);

            // Get the new target position in the bounce direction
            targetPosition = startPosition + bounceDirectionLocalSpace * Vector3.Distance(startPosition, targetPosition);

            // Indicate the bounce "time", so we can account for the adjustment in the update step.
            bouncedAtPercent = percentDone;
        }

        /// <summary>
        /// Determine if the <see cref="UIBlock"/> collides with an interior edge of its parent bounds.
        /// </summary>
        /// <param name="parentHit">The collision result.</param>
        /// <returns>True if a collision is detected, otherwise returns false.</returns>
        private bool CollidesWithParent(out UIBlockHit parentHit)
        {
            // Initialize out param
            parentHit = default;

            if (!UIBlock.gameObject.activeInHierarchy)
            {
                // If the UIBlock is inactive, then
                // we don't need to do anything.
                return false;
            }

            // Get the UIBlock parent
            UIBlock parent = UIBlock.Parent;

            // Making the assumption the UIBlock is a circle, so we only need to check one axis
            float radius = UIBlock.CalculatedSize.X.Value * 0.5f;

            // Get the xy extents of the parent bounds -- this is the bubble's range of movement
            Vector2 parentExtents = parent.CalculatedSize.XY.Value * 0.5f;

            // Since we set the Alignment to Center (in this animation's Update step),
            // UIBlock.CalculatedPosition.Value == UIBlock.transform.localPosition.
            float xPosition = UIBlock.CalculatedPosition.X.Value;
            float yPosition = UIBlock.CalculatedPosition.Y.Value;

            // The sign of the position along each axis
            // will tell us which sides of the parent
            // the bubble is closest too. 
            float horizontalEdgeDirection = Mathf.Sign(xPosition);
            float verticalEdgeDirection = Mathf.Sign(yPosition);

            // Get a position on the parent's closest edge per axis
            float parentEdgeXPosition = horizontalEdgeDirection * parentExtents.x;
            float parentEdgeYPosition = verticalEdgeDirection * parentExtents.y;

            // Determine the bubble's center distance from the closes edge of its parent
            float distanceToHorizontalEdge = Mathf.Abs(parentEdgeXPosition - xPosition);
            float distanceToVerticalEdge = Mathf.Abs(parentEdgeYPosition - yPosition);

            // If the minimum distance to an edge of the parent is less
            // than the bubble radius, then the bubble collides with its parent.
            bool collisionDetected = Mathf.Min(distanceToHorizontalEdge, distanceToVerticalEdge) <= radius;

            if (!collisionDetected)
            {
                // Doesn't collide, nothing to do
                return false;
            }

            // If distance along X is less than distance along Y,
            // then the closest point to the bubble is on the X edge.
            // Otherwise the closest point to the bubble is on the Y edge.
            bool collidesAlongX = distanceToHorizontalEdge < distanceToVerticalEdge;
            Vector3 collisionPointLocalSpace = collidesAlongX ? new Vector3(parentEdgeXPosition, yPosition, 0) : new Vector3(xPosition, parentEdgeYPosition, 0);

            // Convert the collision point into world space
            Vector3 hitPositionWorldSpace = parent.transform.TransformPoint(collisionPointLocalSpace);

            // Calculate the normal vector of the collision
            Vector3 hitNormalWorldSpace = Vector3.Normalize(UIBlock.transform.position - hitPositionWorldSpace);

            // Create a new UIBlock hit with the collision info
            parentHit = new UIBlockHit()
            {
                UIBlock = parent,
                Position = hitPositionWorldSpace,
                Normal = hitNormalWorldSpace,
            };

            // Collision detected
            return true;
        }

        /// <summary>
        /// Read the starting values and generate some target values for the current iteration
        /// </summary>
        private void Initialize()
        {
            // Because we're initializing, ensure this starts from 0
            bouncedAtPercent = 0;

            // The start color is the Gradient's current color
            startColor = UIBlock.Gradient.Color;

            // Generate a random color with full brightness, full saturation, and 50% alpha
            targetColor = Random.ColorHSV(0, 1, 1, 1, 1, 1, 0.5f, 0.5f);

            // The direction to move until a bounce is detected
            Vector3 movementDirection;

            if (targetPosition == startPosition)
            {
                // Generate a random direction to move if
                // targetPosition matches the startPosition,
                // which can occur when the animation is initialized
                // for the very first iteration.
                float x = Random.Range(-1, 1);
                float y = Random.Range(-1, 1);

                movementDirection = new Vector3(x, y, 0).normalized;
            }
            else
            {
                // Maintain the current move direction
                movementDirection = (targetPosition - startPosition).normalized;
            }

            // Start from the current position
            startPosition = UIBlock.transform.localPosition;
            
            // End at DistanceToMove from the startPosition along the movementDirection
            targetPosition = startPosition + movementDirection * DistanceToMove;

            // Start from the current rotation
            startRotation = UIBlock.transform.localRotation;

            // End at a randomly generated rotation
            targetRotation = startRotation * Quaternion.Euler(0, 0, Random.Range(-360, 360));
        }
    }
}
