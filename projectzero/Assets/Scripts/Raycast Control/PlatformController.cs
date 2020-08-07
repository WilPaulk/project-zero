using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController
{
  public LayerMask passengerMask;
  public Vector3 move;
  private Vector3 velocity;

  public override void Start()
  {
    base.Start();
  }

  void Update()
  {
    UpdateRaycastOrigins();

    velocity = move * Time.fixedDeltaTime;
    velocity.x = (Mathf.RoundToInt(velocity.x * 32f)) / 32f;
    velocity.y = (Mathf.RoundToInt(velocity.y * 32f)) / 32f;
  }

  void FixedUpdate()
  {
    MovePassengers(velocity);
    transform.Translate(velocity);
  }

  void MovePassengers(Vector3 velocity)
  {
    HashSet<Transform> movedPassengers = new HashSet<Transform>();
    float directionX = Mathf.Sign(velocity.x);
    float directionY = Mathf.Sign(velocity.y);

    //vertically moving Platform
    if (velocity.y != 0f)
    {
      float rayLength = Mathf.Abs (velocity.y) + skin;

      for (int i = 0; i < verticalRayCount; i++)
      {
        Vector2 rayOrigin = (directionY == -1f)? raycastOrigins.bottomLeft:raycastOrigins.topLeft;
        rayOrigin += Vector2.right * (verticalRaySpace * i);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

        if(hit)
        {
          if (!movedPassengers.Contains(hit.transform))
          {
            movedPassengers.Add(hit.transform);
            float pushX = (directionY == 1f)?velocity.x:0f;
            float pushY = velocity.y - (hit.distance - skin) * directionY;
            hit.transform.Translate(new Vector3(pushX,pushY));
          }
        }
      }
    }
  }
}
