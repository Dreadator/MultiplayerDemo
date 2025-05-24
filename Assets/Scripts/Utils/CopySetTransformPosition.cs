using System.Collections.Generic;
using UnityEngine;

public class CopySetTransformPosition : MonoBehaviour
{
    [SerializeField] List<Transform> transformsToCopy;
    [SerializeField] List<Transform> transformsToSet;

    [ContextMenu("Copy and Set transforms")]
    private void CopyAndSetTransforms() 
    {
        for (int i = 0; i < transformsToCopy.Count; i++)
        {
            transformsToSet[i].SetPositionAndRotation(
                transformsToCopy[i].position,
                transformsToCopy[i].rotation);

            transformsToSet[i].localScale = transformsToCopy[i].localScale;
        }
    }
}
