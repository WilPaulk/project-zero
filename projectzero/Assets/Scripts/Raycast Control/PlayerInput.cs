using UnityEngine;

[RequireComponent (typeof (Controller2D))]
public class PlayerInput : MonoBehaviour
{
  public float moveSpeed = 6f;
  public float jumpHeight = 6f;
  public float timeToJumpApex = .6f;

  private float jumpVelocity;
  private float gravity;
  private float horizontalMove;
  private float jump;
  private Vector3 velocity;

  public Animator animator;
  public Controller2D controller;

  void Start()
  {
      controller = GetComponent<Controller2D>();
      animator = GetComponent<Animator>();
      gravity = -(2 * jumpHeight) / Mathf.Pow (timeToJumpApex, 2);
      jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
  }

  void Update()
  {
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

    if (controller.collisions.above || controller.collisions.below)
    {
      velocity.y = 0;
    }

    if (Input.GetButtonDown("Jump") && controller.collisions.below)
    {
      jump = jumpVelocity;
    }
  }

  void FixedUpdate()
  {
    velocity.x = horizontalMove * moveSpeed;
    velocity.y += (gravity * Time.fixedDeltaTime) + jump;
    jump = 0f;
    controller.Move(velocity * Time.fixedDeltaTime);

    animator.SetBool("grounded", controller.collisions.below);
    animator.SetFloat("speed", Mathf.Abs(horizontalMove));
    animator.SetFloat("yvelocity", velocity.y);

  }
}
