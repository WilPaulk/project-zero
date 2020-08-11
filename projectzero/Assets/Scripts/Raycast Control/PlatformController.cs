using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController
{
  public LayerMask passengerMask;
  public Vector3 move;
  private Vector3 velocity;
  List<PassengerMovement> passengerMovement;
  Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

  public override void Start()
  {
    base.Start();
  }

  void Update()
  {
    UpdateRaycastOrigins();
    velocity = move * Time.deltaTime;
    velocity.x = (Mathf.RoundToInt(velocity.x * 32f)) / 32f;
    velocity.y = (Mathf.RoundToInt(velocity.y * 32f)) / 32f;

    CalculatePassengerMovementPre(velocity);
    MovePassengers(true);

    transform.Translate(velocity);

    CalculatePassengerMovementPost(velocity);
    MovePassengers(false);
  }

  void MovePassengers (bool beforeMovePlatform)
  {
    foreach (PassengerMovement passenger in passengerMovement)
    {
      if (!passengerDictionary.ContainsKey(passenger.transform))
      {
        passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
      }
      if (passenger.moveBeforePlatform == beforeMovePlatform)
      {
        passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.platformGrounded, false);
      }
    }
  }

  void CalculatePassengerMovementPre(Vector3 velocity)
  {
    HashSet<Transform> movedPassengers = new HashSet<Transform>();
    passengerMovement = new List<PassengerMovement>();
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
            passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1f, true));
          }
        }
      }
    }
  }

  void CalculatePassengerMovementPost(Vector3 velocity)
  {
    HashSet<Transform> movedPassengers = new HashSet<Transform>();
    passengerMovement = new List<PassengerMovement>();
    float directionX = Mathf.Sign(velocity.x);
    float directionY = Mathf.Sign(velocity.y);

    //passenger on top of a platform w/ y <= 0
    if (directionY == -1f || velocity.y == 0f && velocity.x != 0f)
    {
      float rayLength = skin * 2f + Mathf.Abs(velocity.y);

      for (int i = 0; i < verticalRayCount; i++)
      {
        Vector2 rayOrigin = raycastOrigins.topLeft + (Vector2.right * (verticalRaySpace * i));
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

        if(hit)
        {
          if (!movedPassengers.Contains(hit.transform))
          {
            movedPassengers.Add(hit.transform);
            float pushX = velocity.x;
            float pushY = velocity.y - (hit.distance -skin);
            passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
          }
        }
      }
    }
  }
  struct PassengerMovement
  {
    public Transform transform;
    public Vector3 velocity;
    public bool platformGrounded;
    public bool moveBeforePlatform;

    public PassengerMovement(Transform _transform, Vector3 _velocity, bool _platformGrounded, bool _moveBeforePlatform)
    {
      transform = _transform;
      velocity = _velocity;
      platformGrounded = _platformGrounded;
      moveBeforePlatform = _moveBeforePlatform;
    }
  }
}
