using UnityEngine;

[RequireComponent (typeof (Controller2D))]
public class PlayerInput : MonoBehaviour
{
  public float gravity = -9.8f;
  private Vector3 velocity;

  public Controller2D controller;

  void Start()
  {
      controller = GetComponent<Controller2D>();
  }

  void Update()
  {
    velocity.y += gravity * Time.deltaTime;
    controller.Move(velocity * Time.deltaTime);
  }
}

//gay
