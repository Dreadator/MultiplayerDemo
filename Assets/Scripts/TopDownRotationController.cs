using Unity.Netcode;
using UnityEngine;

public class TopDownRotationController : NetworkBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 90f; // Degrees per second
    [SerializeField] private float maxRotationAngle = 45f; // Maximum rotation in both directions

    private const string HORIZONTAL = "Horizontal";

    private float currentRotation = 0f;
    private float initialRotation = 0f;

    void Start()
    {
        // Initialize current rotation based on the transform's current Z rotation
        initialRotation = transform.localRotation.eulerAngles.z;
    }

    void Update()
    {
        if(IsOwner)
            HandleRotationInput();
    }

    private void HandleRotationInput()
    {
        // Get horizontal input (-1 to 1)
        float rotationInput = Input.GetAxis(HORIZONTAL);

        // Calculate the desired rotation change (negated to fix direction)
        float rotationChange = -rotationInput * rotationSpeed * Time.deltaTime;
        float newRotation = currentRotation + rotationChange;

        // Clamp the rotation within the specified limits
        newRotation = Mathf.Clamp(newRotation, -maxRotationAngle, maxRotationAngle);

        // Only update if the rotation actually changed
        if (Mathf.Abs(newRotation - currentRotation) > 0.001f)
        {
            currentRotation = newRotation;

            // Apply the rotation to the transform's local rotation relative to initial rotation
            float finalRotation = initialRotation + currentRotation;
            transform.localRotation = Quaternion.Euler(0f, 0f, finalRotation);
        }
    }
}