using UnityEngine;

// requires a box collider for rays to be calculated from
[RequireComponent (typeof (BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
  // public vars
  public int horizontalRayCount = 4;
  public int verticalRayCount = 4;
  public const float skin = .015625f;
  [HideInInspector] public float horizontalRaySpace;
  [HideInInspector] public float verticalRaySpace;

  // unity components and custom structs
  public LayerMask collisionMask;
  [HideInInspector] public BoxCollider2D moveCollider;
  public RaycastOrigins raycastOrigins;

  public virtual void Start()
  {
    // assigns box collider for ray origins and calculates spacing
    moveCollider = GetComponent<BoxCollider2D>();
    CalculateRaySpace();
  }

  public void UpdateRaycastOrigins()
  {
    // checks the bounds of game objects box collider and calculates origins of rays based on bounds of that collider and skin width
    Bounds bounds = moveCollider.bounds;
    bounds.Expand (skin * -2);

    raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
    raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
    raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
    raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
  }

  public void CalculateRaySpace()
  {
    //calculates spacing between rays using bounds of game objects box collider, skin width, and number of rays (min of 2 required)
    Bounds bounds = moveCollider.bounds;
    bounds.Expand (skin * -2);

    horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
    verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

    horizontalRaySpace = bounds.size.y / (horizontalRayCount - 1);
    verticalRaySpace = bounds.size.x / (verticalRayCount -1);
  }

  public struct RaycastOrigins
  {
    //stores info on ray origins
    public Vector2 topLeft, topRight;
    public Vector2 bottomLeft, bottomRight;
  }
}
