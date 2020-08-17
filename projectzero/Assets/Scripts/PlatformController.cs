using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// now extends raycast controller
public class PlatformController : RaycastController
{
  // public vars
  public float speed;
  public bool cyclic;
  public float delay;
  [Range(0,2)] public float platSmoothing;

  // private vars

  private int fromWaypointIndex;
  private float percentBetweenWaypoints;
  private float nextMove;

  // unity components, lists, and structs
  public LayerMask passengerMask;
  private Vector3 velocity;
  public Vector3[] localWaypoints;
  Vector3[] globalWaypoints;
  List<PassengerMovement> passengerMovement;
  Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

  // calls base class start method and sets global waypoints in reference to the local waypoints set in the editor
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
    // updates raycast origins, sets velocity with calculation method, rounds velocity to nearest pixel and then moves passengers and platforms depending on priority
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
    // uses a power function so that platforms start and stop slowly and reach max velocity in the middle of their path
    float a = platSmoothing + 1f;
    return Mathf.Pow(x,a) / (Mathf.Pow(x, a) + Mathf.Pow(1f-x, a));
  }

  Vector3 CalculatePlatformMovement()
  {
    // a bit of a complicated one here - it effectively interpolates platform positions based on speed and composite vectors made by comparing the two adjacent waypoints. also contains logic for iterating through waypoints based on either reverse paths or cycles depending on preference set in editor. it handles platform delay and calls the platform smoothing method in line. after all of the math, it returns a velocity to move both the platforms and passengers.
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
    // grabs controller script of any passengers and moves them using move method based on platform velocity - uses a passenger dictionary to store controllers so that we aren't making multiple expensive calls - passes in false in the move call to disable vertical collision detection which causes smooth falling bugs and prevents calling the flip method
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
    // calculates passenger movement for any movement type where passenger takes priority over platform movement (e.g. pushes) - types of movement called out in comments below - uses a hashset to prevent moving passengers twice when they are hit by multiple rays - stores passenger movements in a list to be used by the move passengers method
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
    // calculates passenger movement for any movement type where platform takes priority over passenger movement (e.g. pulls) - types of movement called out in comments below - uses a hashset to prevent moving passengers twice when they are hit by multiple rays - stores passenger movements in a list to be used by the move passengers method
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
    // stores data on passenger movement calculated by passenger movement calculation mathods in a custom object which is populated in passenger movement list and used to call move method
    public Transform transform;
    public Vector3 velocity;
    public bool platformGrounded;
    public bool moveBeforePlatform;

    // structor for creating passenger movement objects
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
    // will disable later - draws waypoints for platforms on screen for better visualization of platform paths and locations during level construction
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
