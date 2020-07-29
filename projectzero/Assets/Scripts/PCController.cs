using UnityEngine;
using UnityEngine.Events;

public class PCController : MonoBehaviour
{
  [SerializeField] private float m_MoveSpeed = 10f;
  [SerializeField] private float m_JumpHeight = 20f;
  [SerializeField] private LayerMask m_whatIsGround;
  [SerializeField] private BoxCollider2D groundBox;
  private Rigidbody2D m_Rigidbody2D;
  private bool m_faceRight = true;
  const float k_GroundedRadius = .001f;
  private bool m_Grounded;
  private bool m_stillGrounded = false;

  [Header("Events")]
  public UnityEvent OnLandEvent;
  public UnityEvent OnAirEvent;

  private void Awake()
  {
    m_Rigidbody2D = GetComponent<Rigidbody2D>();
    if (OnLandEvent == null)
    {
      OnLandEvent = new UnityEvent();
    }
    if (OnAirEvent == null)
    {
      OnAirEvent = new UnityEvent();
    }
  }

  private void FixedUpdate()
  {
    bool wasGrounded = m_Grounded;
    m_Grounded = false;

    RaycastHit2D raycastHit = Physics2D.BoxCast(groundBox.bounds.center, groundBox.bounds.size, 0f, Vector2.down, .01f, m_whatIsGround);
    if (raycastHit.collider != null)
    {
      m_Grounded = true;
      if (!wasGrounded)
      {
        m_stillGrounded = true;
        OnLandEvent.Invoke();
      }
    }
    else
    {
      if (m_stillGrounded)
      {
        m_stillGrounded = false;
        OnAirEvent.Invoke();
      }
    }
  }


  public void Move(float horizontalMove, bool jump)
  {
    Vector2 targetV = new Vector2(horizontalMove * m_MoveSpeed, m_Rigidbody2D.velocity.y);
    m_Rigidbody2D.velocity = targetV;

    if (horizontalMove > 0 && !m_faceRight)
    {
      flip();
    }
    else if (horizontalMove < 0 && m_faceRight)
    {
      flip();
    }
    if(m_Grounded && jump)
    {
      m_Grounded = false;
      m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpHeight));
    }
  }
  private void flip ()
  {
    m_faceRight = !m_faceRight;
    Vector2 theScale = transform.localScale;
    theScale.x *= -1;
    transform.localScale = theScale;
  }
}
