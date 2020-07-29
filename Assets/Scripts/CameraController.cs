using UnityEngine;

public class CameraController : MonoBehaviour
{
  [SerializeField] private Transform parent;
  [SerializeField] private Transform camera;
  [SerializeField] private float offset = -10;
  private Vector3 temp = Vector3.zero;

  private void LateUpdate()
  {
    temp = new Vector3 (parent.position.x, parent.position.y, parent.position.z + offset);
    camera.position = temp;
  }
}
