using UnityEngine;

public class GridPosition : MonoBehaviour
{
    [SerializeField] private Vector2Int gridPosition;

    private void OnMouseDown()
    {
        Debug.Log($"clicked: {gridPosition}");
        GameManager.Instance.ClickedOnGridPositionRPC(gridPosition.x, gridPosition.y, GameManager.Instance.localPlayerType);
    }
}
