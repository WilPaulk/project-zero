using UnityEngine;

// requires Controller 2D script
[RequireComponent (typeof (Controller2D))]
public class PlayerInput : MonoBehaviour
{
  //public vars
  public float moveSpeed = 6f;
  public float dashSpeed = 15f;
  public float dashTime = 15f;
  public float maxJumpHeight = 2.3f;
  public float minJumpHeight = .5f;
  public float doubleJumpHeight = 6f;
  public float timeToJumpApex = .6f;
  public float timeToDoubleJumpApex = .6f;
  public float stillDash;
  public float dashCooldownTime = 10f;
  public float wallSlideMaxSpeed = 5f;
  public Vector2 wallJumpVelocity;
  public float sideJumpSmooth;

  //private vars
  private float maxJumpVelocity;
  private float minJumpVelocity;
  private float doubleJumpVelocity;
  private float gravity;
  private float doubleJumpGravity;
  private float horizontalMove;
  private float jump;
  private float doubleJump;
  private bool doubleJumpAnim;
  private bool doubleJumpReady = false;
  private float dash;
  private bool dashAnim;
  private bool dashReady = false;
  private float dashCooldown = 0f;
  private Vector3 velocity;
  private bool doubleJumpActive;
  private bool wallSliding;
  private float sideJump;
  private float doubleJumpReset;
  private bool fall;

  //unity components
  public Animator animator;
  public Controller2D controller;

  void Start()
  {
    //grabs needed components from player object and calculates physics
    controller = GetComponent<Controller2D>();
    animator = GetComponent<Animator>();
    gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
    doubleJumpGravity = -(2 * doubleJumpHeight) / Mathf.Pow (timeToDoubleJumpApex, 2);
    maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    doubleJumpVelocity = Mathf.Abs(doubleJumpGravity) * timeToDoubleJumpApex;
    stillDash = 0f;
  }

  void Update()
  {
    //gets controller input and translates to standard values every frame
    if (Input.GetAxisRaw("Horizontal") > .2 && Time.time > dashCooldown)
    {
      horizontalMove = 1f;
    }
    else if (Input.GetAxisRaw("Horizontal") < -.2 && Time.time > dashCooldown)
    {
      horizontalMove = -1f;
    }
    else
    {
      horizontalMove = 0f;
    }

    //adds logic for wall sliding which will enable new animations and wall jumping
    wallSliding = false;
    if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0f)
    {
      wallSliding = true;
      if (velocity.y < -wallSlideMaxSpeed)
      {
        velocity.y = -wallSlideMaxSpeed;
      }
    }

    //allows double jump if in the air and have already jumped - based on this design you cannot double jump from a fall
    if (Input.GetButtonDown("Jump") && doubleJumpReady && !controller.collisions.grounded && Time.time > dashCooldown)
    {
      jump = doubleJumpVelocity;
      doubleJumpAnim = true;
      doubleJumpReady = false;
      // i'm an idiot
      velocity.y = 0f;
    }

    // sets jump if jump conditions are met - also added logic for wall jumping -  added a bug fix to disable a double jump after storing one by single jumping and falling off a ledge, uses double jump apex * 2 so that if we change the timing of the jump this reset works automatically
    if (Input.GetButtonDown("Jump") && controller.collisions.grounded && Time.time > dashCooldown && Input.GetAxisRaw("Vertical") > -.9f)
    {
      jump = maxJumpVelocity;
      doubleJumpReady = true;
      doubleJumpReset = Time.time + (2f * timeToJumpApex);
    }
    if (Time.time >= doubleJumpReset)
    {
      if(controller.collisions.grounded)
      {
        doubleJumpReady = false;
      }
    }

    if (Input.GetButtonDown("Jump") && wallSliding)
    {
      sideJump = ((controller.faceRight)? -1f:1f) * wallJumpVelocity.x;
      velocity.y = 0f;
      jump = wallJumpVelocity.y;
    }

    if (Input.GetButtonDown("Jump") && Input.GetAxisRaw("Vertical") <= -.9f && controller.collisions.grounded)
    {
      fall = true;
      Invoke("FallReset", .5f);
    }

    if (Input.GetButtonUp("Jump"))
    {
      if (velocity.y > minJumpVelocity)
      {
        velocity.y = minJumpVelocity;
      }
    }

    // enables dashing and sets dash speed
    if (Input.GetButtonDown("Dash") && Time.time > stillDash && dashReady && Time.time > dashCooldown)
    {
      dashReady = false;
      if (controller.faceRight)
      {
        dash = dashSpeed;
        stillDash = Time.time + dashTime;
      }
      else
      {
        dash = (dashSpeed * -1);
        stillDash = Time.time + dashTime;
      }
    }
  // }

  // void FixedUpdate()
  // {
    //takes in relavent vars to determine movement and calls move function at fixed intervals; additionally handles timing of certain movement abilities
    if (Mathf.Abs(dash) > 0f)
    {
      velocity.x = dash;
      dashAnim = true;
      if (controller.collisions.grounded)
      {
        velocity.y += (gravity * Time.deltaTime);
      }
      if (!controller.collisions.grounded)
      {
        velocity.y = 0f;
      }
      if (Time.time > stillDash)
      {
        dash = 0f;
        dashAnim = false;
        if (controller.collisions.grounded)
        {
          dashCooldown = Time.time + dashCooldownTime;
        }
      }
    }
    else
    {
      velocity.x = (horizontalMove * moveSpeed) + sideJump;
      velocity.y += (gravity * Time.deltaTime) + jump;
      if (controller.collisions.grounded)
      {
        dashReady = true;
      }
    }
    // resets jump and adds editable smoothing to side jump since it is not smoothly decreased by gravity like a vertical jump
    jump = 0f;
    if (Mathf.Abs(sideJump) > 0f)
    {
      float sideSign = Mathf.Sign(sideJump);
      sideJump = (Mathf.Abs(sideJump) - sideJumpSmooth) * sideSign;

      if (Mathf.Abs(sideJump) <= 0f)
      {
        sideJump = 0f;
      }
    }
    controller.Move(velocity * Time.deltaTime, false, fall, true);

    // resets velocity to 0 to stop gravity accumulation when at rest
    if (controller.collisions.above || controller.collisions.below)
    {
      velocity.y = 0f;
    }
  }

  void FixedUpdate()
  {
    // sets animator parameters each frame
    animator.SetBool("grounded", controller.collisions.grounded);
    animator.SetFloat("speed", Mathf.Abs(horizontalMove))  ;
    animator.SetFloat("yvelocity", velocity.y);
    animator.SetBool("doublejump", doubleJumpAnim);
    animator.SetBool("dash", dashAnim);
    animator.SetBool("wall slide", wallSliding);
    doubleJumpAnim = false;
  }

  void FallReset()
  {
    fall = false;
  }
}
