using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController
{
  public LayerMask passengerMask;
  private Vector3 velocity;
  public float speed;
  private int fromWaypointIndex;
  private float percentBetweenWaypoints;
  public bool cyclic;
  public float delay;
  private float nextMove;
  [Range(0,2)] public float platSmoothing;

  public Vector3[] localWaypoints;
  Vector3[] globalWaypoints;

  List<PassengerMovement> passengerMovement;
  Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

  public override void Start()
  {
    base.Start();
    globalWaypoints = new Vector3[localWaypoints.Length];
    for (int i =0; i < localWaypoints.Length; i ++)
    {
      globalWaypoints[i] = localWaypoints[i] + transform.position;
    }
  }

  void FixedUpdate()
  {
    UpdateRaycastOrigins();
    velocity = CalculatePlatformMovement();
    velocity.x = (Mathf.RoundToInt(velocity.x * 32f)) / 32f;
    velocity.y = (Mathf.RoundToInt(velocity.y * 32f)) / 32f;

    CalculatePassengerMovementPre(velocity);
    MovePassengers(true);

    transform.Translate(velocity);

    CalculatePassengerMovementPost(velocity);
    MovePassengers(false);
  }

  float PlatSmooth(float x)
  {
    float a = platSmoothing + 1f;
    return Mathf.Pow(x,a) / (Mathf.Pow(x, a) + Mathf.Pow(1f-x, a));
  }

  Vector3 CalculatePlatformMovement()
  {
    if (Time.time < nextMove)
    {
      return Vector3.zero;
    }

    int toWaypointIndex = fromWaypointIndex +1;
    if (cyclic)
    {
      if (fromWaypointIndex == globalWaypoints.Length - 1)
      {
        toWaypointIndex = 0;
      }
    }
    float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
    percentBetweenWaypoints += Time.fixedDeltaTime * speed/distanceBetweenWaypoints;
    percentBetweenWaypoints = Mathf.Clamp(percentBetweenWaypoints, 0f, 1f);
    float smoothedPercentBetweenWaypoints = PlatSmooth(percentBetweenWaypoints);
    Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], smoothedPercentBetweenWaypoints);

    if (percentBetweenWaypoints >= 1f)
    {
      percentBetweenWaypoints = 0f;
      fromWaypointIndex ++;

      if (cyclic)
      {
        if (fromWaypointIndex > globalWaypoints.Length - 1)
        {
          fromWaypointIndex = 0;
        }
      }
      else
      {
        if (fromWaypointIndex >= globalWaypoints.Length - 1)
        {
          fromWaypointIndex = 0;
          System.Array.Reverse(globalWaypoints);
        }
      }
      nextMove = Time.time + delay;
    }
    return (newPos - transform.position);
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

  void OnDrawGizmos()
  {
    if (localWaypoints != null)
    {
      Gizmos.color = Color.red;
      float size = .3f;

      for (int i=0; i < localWaypoints.Length; i++)
      {
        Vector3 globalWaypointPos = (Application.isPlaying)? globalWaypoints[i]: localWaypoints[i] + transform.position;
        Gizmos.DrawLine(globalWaypointPos - Vector3.up *  size, globalWaypointPos + Vector3.up * size);
        Gizmos.DrawLine(globalWaypointPos - Vector3.left *  size, globalWaypointPos + Vector3.left * size);
      }
    }
  }
}
