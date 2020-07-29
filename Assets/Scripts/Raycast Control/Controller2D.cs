using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
  private const float skin = .01f;
  public int horizontalRayCount = 4;
  public int verticalRayCount = 4;

  private float horizontalRaySpace;
  private float verticalRaySpace;

  public BoxCollider2D moveCollider;
  private RaycastOrigins raycastOrigins;

  void Start()
  {
    moveCollider = GetComponent<BoxCollider2D>();
    CalculateRaySpace();
  }

  public void Move(Vector3 velocity)
  {
    UpdateRaycastOrigins();

    VerticalCollisions (ref velocity);

    transform.Translate (velocity);
  }

  void VerticalCollisions(ref Vector3 velocity)
  {
    for (int i = 0; i < verticalRayCount; i++)
    {
      Debug.DrawRay (raycastOrigins.bottomLeft + Vector2.right * verticalRaySpace * i, Vector2.up * -2, Color.red);
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

  struct RaycastOrigins
  {
    public Vector2 topLeft, topRight;
    public Vector2 bottomLeft, bottomRight;
  }
}
