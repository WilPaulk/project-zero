using UnityEngine;

// requires Controller 2D script
[RequireComponent (typeof (Controller2D))]
public class PlayerInput : MonoBehaviour
{
  //public vars
  public float moveSpeed = 6f;
  public float dashSpeed = 15f;
  public float dashTime = 15f;
  public float jumpHeight = 6f;
  public float doubleJumpHeight = 6f;
  public float timeToJumpApex = .6f;
  public float timeToDoubleJumpApex = .6f;
  public float stillDash;
  public float dashCooldownTime = 10f;

  //private vars
  private float jumpVelocity;
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

  //unity components
  public Animator animator;
  public Controller2D controller;

  void Start()
  {
    //grabs needed components from player object and calculates physics
    controller = GetComponent<Controller2D>();
    animator = GetComponent<Animator>();
    gravity = -(2 * jumpHeight) / Mathf.Pow (timeToJumpApex, 2);
    doubleJumpGravity = -(2 * doubleJumpHeight) / Mathf.Pow (timeToDoubleJumpApex, 2);
    jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    doubleJumpVelocity = Mathf.Abs(doubleJumpGravity) * timeToDoubleJumpApex;
    stillDash = 0f;
  }

  void Update()
  {
    //gets controller input and translates to standard values every frame
    if (Input.GetAxisRaw("Horizontal") > .2 && dashCooldown == 0f)
    {
      horizontalMove = 1f;
    }
    else if (Input.GetAxisRaw("Horizontal") < -.2 && dashCooldown == 0f)
    {
      horizontalMove = -1f;
    }
    else
    {
      horizontalMove = 0f;
    }

    // resets velocity to 0 to stop gravity accumulation when at rest
    if (controller.collisions.above || controller.collisions.below)
    {
      velocity.y = 0f;
    }

    //allows double jump if in the air and have already jumped - based on this design you cannot double jump from a fall
    if (Input.GetButtonDown("Jump") && doubleJumpReady && !controller.collisions.grounded && dashCooldown == 0f)
    {
      jump = doubleJumpVelocity;
      doubleJumpAnim = true;
      doubleJumpReady = false;
      // i'm an idiot
      velocity.y = 0f;
    }

    // sets jump if jump conditions are met
    if (Input.GetButtonDown("Jump") && controller.collisions.grounded && dashCooldown == 0f)
    {
      jump = jumpVelocity;
      doubleJumpReady = true;
    }

    // enables dashing and sets dash speed
    if (Input.GetButtonDown("Dash") && stillDash == 0f && dashReady && dashCooldown == 0f)
    {
      dashReady = false;
      if (controller.faceRight)
      {
        dash = dashSpeed;
        stillDash = dashTime;
      }
      else
      {
        dash = (dashSpeed * -1);
        stillDash = dashTime;
      }
    }
  }

  void FixedUpdate()
  {
    //takes in relavent vars to determine movement and calls move function at fixed intervals; additionally handles timing of certain movement abilities
    if (Mathf.Abs(dash) > 0f)
    {
      velocity.x = dash;
      dashAnim = true;
      if (controller.collisions.grounded)
      {
        velocity.y += (gravity * Time.fixedDeltaTime);
      }
      if (!controller.collisions.grounded)
      {
        velocity.y = 0f;
      }
      if (stillDash >= 1f)
      {
        stillDash -= 1f;
      }
      else
      {
        dash = 0f;
        dashAnim = false;
        if (controller.collisions.grounded)
        {
          dashCooldown = dashCooldownTime;
        }
      }
    }
    else
    {
      velocity.x = (horizontalMove * moveSpeed);
      velocity.y += (gravity * Time.fixedDeltaTime) + jump;
      if (controller.collisions.grounded)
      {
        dashReady = true;
      }
      if (dashCooldown >= 1f)
      {
        dashCooldown -= 1f;
      }
    }

    jump = 0f;
    controller.Move(velocity * Time.fixedDeltaTime);

    // sets animator parameters each frame
    animator.SetBool("grounded", controller.collisions.grounded);
    animator.SetFloat("speed", Mathf.Abs(horizontalMove))  ;
    animator.SetFloat("yvelocity", velocity.y);
    animator.SetBool("doublejump", doubleJumpAnim);
    animator.SetBool("dash", dashAnim);
    doubleJumpAnim = false;
  }
}
