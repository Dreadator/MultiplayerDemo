using UnityEngine;

public class RotateMoveToTarget : MonoBehaviour
{
    [SerializeField] bool shouldRotate;
    [SerializeField] float rotationSpeed;
    [Space]
    [SerializeField] bool shouldMove;
    [SerializeField] float moveSpeed;

    private Vector2 direction;

    void Update()
    {
        if (shouldRotate)
        {
            direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        }

        if (shouldMove)
        {
            Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = Vector2.MoveTowards(transform.position, cursorPos, moveSpeed * Time.deltaTime);
        }
    }
}