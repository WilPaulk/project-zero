﻿using UnityEngine;

// requires a box collider for rays to be calculated from
[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
  // public vars
  public int horizontalRayCount = 4;
  public int verticalRayCount = 4;
  public float maxClimbAngle = 70f;
  public float maxDescendAngle = 70f;

  // private vars
  private const float skin = .015f;
  private float horizontalRaySpace;
  private float verticalRaySpace;
  private bool faceRight = true;

  // unity components and custom structs
  public LayerMask collisionMask;
  public BoxCollider2D moveCollider;
  private RaycastOrigins raycastOrigins;
  public CollisionInfo collisions;

  void Start()
  {
    // assigns box collider for ray origins and calculates spacing
    moveCollider = GetComponent<BoxCollider2D>();
    CalculateRaySpace();
  }

  public void Move(Vector3 velocity)
  {
    // calls ray origins method, resets collision bools, calls flip method, and sets x and y velocity if moving
    UpdateRaycastOrigins();
    collisions.Reset();
    collisions.velocityOld = velocity;

    if (velocity.x > 0 && !faceRight)
    {
      Flip();
    }

    if (velocity.x < 0 && faceRight)
    {
      Flip();
    }

    if (velocity.y < 0)
    {
      DescendSlope(ref velocity);
    }

    if (velocity.x != 0)
    {
      HorizontalCollisions(ref velocity);
    }

    if (velocity.y != 0)
    {
      VerticalCollisions (ref velocity);
    }

    transform.Translate (velocity);
  }

  void HorizontalCollisions(ref Vector3 velocity)
  {
    // checks if there are collisions with platform colliders in direction of horizontal movement withing velocity range for current frame and adjusts velocity so that it does not exceed that range; also updates collision bools
    float directionX = Mathf.Sign (velocity.x);
    float rayLength = Mathf.Abs (velocity.x) + skin;

    for (int i = 0; i < horizontalRayCount; i++)
    {
      Vector2 rayOrigin = (directionX == -1f)? raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
      rayOrigin += Vector2.up * (horizontalRaySpace * i);
      RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

      if (hit)
      {
        //for any horizontal hit angle is captured
        float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
        //if that angle is hit by the bottom ray and is climbable
        if (i == 0 && slopeAngle <= maxClimbAngle)
        {
          //if we start ascending a slope while we are descending a slope, stop descending and reset velocity to initial velocity to avoid slowing down in valleys
          if (collisions.descendingSlope)
          {
            collisions.descendingSlope = false;
            velocity = collisions.velocityOld;
          }
          float distanceToSlopeStart = 0f;
          //check to see if is a new slope; if so decrese velocity by distance to the slope start and use remaining distance to call climb slope
          if (slopeAngle != collisions.slopeAngleOld)
          {
            distanceToSlopeStart = hit.distance - skin;
            velocity.x -= distanceToSlopeStart * directionX;
          }
          //call climb slope and after it returns add back in distance to slope start
          ClimbSlope (ref velocity, slopeAngle);
          velocity.x += distanceToSlopeStart * directionX;
        }

        // if we experience a collision with a wall while we are not climbing a slope or we are climbing a slope into a wall
        if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
        {
          // set x velocity and ray length to hit distance
          velocity.x = (hit.distance - skin) * directionX;
          rayLength = hit.distance;
          // if we experience a collision with a wall while climbing a slope adjust y velocity by trig for smooth collision
          if (collisions.climbingSlope)
          {
            velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
          }

          //set collision struct bools
          collisions.left = directionX == -1;
          collisions.right = directionX == 1;
        }
      }
    }
  }

  void ClimbSlope (ref Vector3 velocity, float slopeAngle)
  {
    // calculate new y velocity based on trig of angle
    float moveDistance = Mathf.Abs(velocity.x);
    float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

    //ensure that climb velocity Y is greater than current Y velocity (to prevent sticking to slope when jumping), if so, set new trig values for velocity x and y and set collisions struct features based on climbing slope
    if (velocity.y <= climbVelocityY)
    {
      velocity.y = climbVelocityY;
      velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
      collisions.below = true;
      collisions.climbingSlope = true;
      collisions.slopeAngle = slopeAngle;
    }
  }

  void DescendSlope (ref Vector3 velocity)
  {
    float directionX = Mathf.Sign(velocity.x);
    Vector2 rayOrigin = (directionX == -1)? raycastOrigins.bottomRight:raycastOrigins.bottomLeft;
    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);

    if (hit)
    {
      float slopeAngle = Vector2.Angle(hit.normal , Vector2.up);
      if (slopeAngle != 0f && slopeAngle <= maxDescendAngle)
      {
        if (Mathf.Sign(hit.normal.x) == directionX)
        {
          if (hit.distance - skin <=  Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
          {
            float moveDistance = Mathf.Abs(velocity.x);
            float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            velocity.y -= descendVelocityY;

            collisions.slopeAngle = slopeAngle;
            collisions.descendingSlope = true;
            collisions.below = true;
          }
        }
      }
    }
  }

  void VerticalCollisions(ref Vector3 velocity)
  {
    // checks if there are collisions with platform colliders in direction of vertical movement withing velocity range for current frame and adjusts velocity so that it does not exceed that range for each ray iteratively; also updates collision bools
    float directionY = Mathf.Sign (velocity.y);
    float rayLength = Mathf.Abs (velocity.y) + skin;

    for (int i = 0; i < verticalRayCount; i++)
    {
      Vector2 rayOrigin = (directionY == -1f)? raycastOrigins.bottomLeft:raycastOrigins.topLeft;
      rayOrigin += Vector2.right * (verticalRaySpace * i + velocity.x);
      RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

      if (hit)
      {
        velocity.y = (hit.distance - skin) * directionY;
        rayLength = hit.distance;

        //if we have a vertical collision while climbing a slope, adjust x velocity with trig for smooth collision
        if (collisions.climbingSlope)
        {
          velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
        }

        // update collisions struct bools
        collisions.below = directionY == -1;
        collisions.above = directionY == 1;
      }
    }
    // if we are climbing a slope
    if (collisions.climbingSlope)
    {
      //cast a ray from the next frame's y position to detect a change in slope and modify velocity accordingly to keep from being caught in the corner of two changing climbable slopes; also update slope angle to new slope in struct
      float directionX = Mathf.Sign(velocity.x);
      rayLength = Mathf.Abs(velocity.x) + skin;
      Vector2 rayOrigin = ((directionX == -1)? raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + Vector2.up * velocity.y;
      RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

      if (hit)
      {
        float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
        if (slopeAngle != collisions.slopeAngle)
        {
          velocity.x = (hit.distance - skin) * directionX;
          collisions.slopeAngle = slopeAngle;
        }
      }
    }
  }

  void UpdateRaycastOrigins()
  {
    // checks the bounds of game objects box collider and calculates origins of rays based on bounds of that collider and skin width
    Bounds bounds = moveCollider.bounds;
    bounds.Expand (skin * -2);

    raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
    raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
    raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
    raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
  }

  void CalculateRaySpace()
  {
    //calculates spacing between rays using bounds of game objects box collider, skin width, and number of rays (min of 2 required)
    Bounds bounds = moveCollider.bounds;
    bounds.Expand (skin * -2);

    horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
    verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

    horizontalRaySpace = bounds.size.y / (horizontalRayCount - 1);
    verticalRaySpace = bounds.size.x / (verticalRayCount -1);
  }

  private void Flip ()
  {
    // flips the character left and right and updates bool that determines which direction the character is facing
    faceRight = !faceRight;
    Vector2 theScale = transform.localScale;
    theScale.x *= -1;
    transform.localScale = theScale;
  }

  struct RaycastOrigins
  {
    //stores info on ray origins
    public Vector2 topLeft, topRight;
    public Vector2 bottomLeft, bottomRight;
  }

  public struct CollisionInfo
  {
    // stores info on collisions detected and provides a method for resetting all bools in struct
    public bool above, below;
    public bool left, right;
    public bool climbingSlope;
    public float slopeAngle, slopeAngleOld;
    public bool descendingSlope;
    public Vector3 velocityOld;

    public void Reset()
    {
      above = below = false;
      left = right = false;
      climbingSlope = false;
      descendingSlope = false;
      slopeAngleOld = slopeAngle;
      slopeAngle = 0f;
    }
  }
}
