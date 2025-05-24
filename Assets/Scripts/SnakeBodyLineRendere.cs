using UnityEngine;

public class SnakeBodyLineRendere : MonoBehaviour
{
    [SerializeField] int length;
    [SerializeField] LineRenderer lineRenderer;

    [SerializeField] Vector3[] segmentPoses;

    [SerializeField] Transform targetTransform;

    [SerializeField] float smoothSpeed;
    [SerializeField] float targetDistance;

    private Vector3[] smoothDefVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer.positionCount = length;
        segmentPoses = new Vector3[length];
        smoothDefVelocity = new Vector3[length];
    }

    // Update is called once per frame
    void Update()
    {
        segmentPoses[0] = targetTransform.position;

        for (int i = 1; i < segmentPoses.Length; i++) 
        {
            segmentPoses[i] = Vector3.SmoothDamp(segmentPoses[i], segmentPoses[i - 1] + targetTransform.right * targetDistance, ref smoothDefVelocity[i], smoothSpeed);
        }
        lineRenderer.SetPositions(segmentPoses);
    }
}
