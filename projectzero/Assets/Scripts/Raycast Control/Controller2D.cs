using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
  private const float skin = .015f;
  public int horizontalRayCount = 4;
  public int verticalRayCount = 4;

  private float horizontalRaySpace;
  private float verticalRaySpace;
  private bool faceRight = true;

  public LayerMask collisionMask;
  public BoxCollider2D moveCollider;
  private RaycastOrigins raycastOrigins;
  public CollisionInfo collisions;

  void Start()
  {
    moveCollider = GetComponent<BoxCollider2D>();
    CalculateRaySpace();
  }

  public void Move(Vector3 velocity)
  {
    UpdateRaycastOrigins();
    collisions.Reset();

    if (velocity.x > 0 && !faceRight)
    {
      Flip();
    }

    if (velocity.x < 0 && faceRight)
    {
      Flip();
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
    float directionX = Mathf.Sign (velocity.x);
    float rayLength = Mathf.Abs (velocity.x) + skin;

    for (int i = 0; i < horizontalRayCount; i++)
    {
      Vector2 rayOrigin = (directionX == -1f)? raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
      rayOrigin += Vector2.up * (horizontalRaySpace * i);
      RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

      if (hit)
      {
        velocity.x = (hit.distance - skin) * directionX;
        rayLength = hit.distance;
        collisions.left = directionX == -1;
        collisions.right = directionX == 1;
    }
  }
}

  void VerticalCollisions(ref Vector3 velocity)
  {
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
        collisions.below = directionY == -1;
        collisions.above = directionY == 1;
      }
    }
  }

  void UpdateRaycastOrigins()
  {
    Bounds bounds = moveCollider.bounds;
    bounds.Expand (skin * -2);

    raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
    raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
    raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
    raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
  }

  void CalculateRaySpace()
  {
    Bounds bounds = moveCollider.bounds;
    bounds.Expand (skin * -2);

    horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
    verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

    horizontalRaySpace = bounds.size.y / (horizontalRayCount - 1);
    verticalRaySpace = bounds.size.x / (verticalRayCount -1);
  }

  private void Flip ()
  {
    faceRight = !faceRight;
    Vector2 theScale = transform.localScale;
    theScale.x *= -1;
    transform.localScale = theScale;
  }

  struct RaycastOrigins
  {
    public Vector2 topLeft, topRight;
    public Vector2 bottomLeft, bottomRight;
  }

  public struct CollisionInfo
  {
    public bool above, below;
    public bool left, right;

    public void Reset()
    {
      above = below = false;
      left = right = false;
    }
  }
}
