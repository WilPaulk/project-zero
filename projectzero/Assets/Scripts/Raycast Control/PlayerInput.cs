using UnityEngine;

// requires Controller 2D script
[RequireComponent (typeof (Controller2D))]
public class PlayerInput : MonoBehaviour
{
  //public vars
  public float moveSpeed = 6f;
  public float jumpHeight = 6f;
  public float timeToJumpApex = .6f;

  //private vars
  private float jumpVelocity;
  private float gravity;
  private float horizontalMove;
  private float jump;
  private Vector3 velocity;

  //unity components
  public Animator animator;
  public Controller2D controller;

  void Start()
  {
    //grabs needed components from player object and calculates physics
    controller = GetComponent<Controller2D>();
    animator = GetComponent<Animator>();
    gravity = -(2 * jumpHeight) / Mathf.Pow (timeToJumpApex, 2);
    jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
  }

  void Update()
  {
    //gets controller input and translates to standard values
    if (Input.GetAxisRaw("Horizontal") > .2)
    {
      horizontalMove = 1f;
    }
    else if (Input.GetAxisRaw("Horizontal") < -.2)
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
      velocity.y = 0;
    }

    // sets jump if jump conditions are met
    if (Input.GetButtonDown("Jump") && controller.collisions.below)
    {
      jump = jumpVelocity;
    }
  }

  void FixedUpdate()
  {
    //takes in relavent vars to determine movement and calls move function each frame
    velocity.x = horizontalMove * moveSpeed;
    velocity.y += (gravity * Time.fixedDeltaTime) + jump;
    jump = 0f;
    controller.Move(velocity * Time.fixedDeltaTime);

    // sets animator parameters each frame
    animator.SetBool("grounded", controller.collisions.below);
    animator.SetFloat("speed", Mathf.Abs(horizontalMove))  ;
    animator.SetFloat("yvelocity", velocity.y);
  }
}
