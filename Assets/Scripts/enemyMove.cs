using UnityEngine;

public class enemyMove : MonoBehaviour
{
  private Rigidbody2D m_Rigidbody2D;
  [SerializeField] private float moveSpeedX = 1f;
  [SerializeField] private float moveSpeedY = 1f;
  [SerializeField] private float moveRangeX = 1f;
  [SerializeField] private float moveRangeY = 1f;
  private float xcounter = 0f;
  private float ycounter = 0f;
  private Vector2 currentPosition = Vector2.zero;
  private Vector2 newPosition = Vector2.zero;
  private Transform m_Transform;


  private void Awake()
  {
    m_Rigidbody2D = GetComponent<Rigidbody2D>();
    m_Transform = GetComponent<Transform>();
  }

  void FixedUpdate()
  {
    if (xcounter == moveRangeX)
    {
      moveSpeedX = moveSpeedX * -1;
      xcounter = 0;
    }
    if (ycounter == moveRangeY)
    {
      moveSpeedY = moveSpeedY * -1;
      ycounter = 0;
    }
    currentPosition = m_Transform.position;
    newPosition = new Vector2 (currentPosition.x + moveSpeedX, currentPosition.y + moveSpeedY);
    xcounter += 1;
    ycounter += 1;
    m_Transform.position = newPosition;
  }
}
