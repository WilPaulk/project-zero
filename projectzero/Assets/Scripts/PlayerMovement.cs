using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  public PCController controller;
  public Animator animator;
  private float horizontalMove = 0f;
  private bool jump = false;
  public Rigidbody2D m_Rigidbody2D;


  private void Update()
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

    if (Input.GetButtonDown("Jump"))
    {
      jump = true;
    }

  }

  private void FixedUpdate ()
  {
    controller.Move (horizontalMove * Time.fixedDeltaTime, jump);
    jump = false;
    animator.SetFloat("speed", Mathf.Abs(horizontalMove));
    animator.SetFloat("yv", m_Rigidbody2D.velocity.y);
  }

  public void OnLand ()
  {
    animator.SetBool("grounded", true);
  }

  public void OnAir ()
  {
    animator.SetBool("grounded", false);
  }
}
