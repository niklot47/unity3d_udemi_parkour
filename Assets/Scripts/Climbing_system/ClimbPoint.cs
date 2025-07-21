using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ClimbPoint : MonoBehaviour
{
    [SerializeField] List<Neighbor> neighbors;

    public void Awake()
    {
        foreach (var neighbor in neighbors.Where(n => n.isTwoWay))
        {
            neighbor.point?.CreateConnection(this, -neighbor.direction, neighbor.connectionType, neighbor.isTwoWay);
        }
    }

    public void CreateConnection(ClimbPoint climbPoint, Vector2 direction, ConnectionType connectionType, bool isTwoWay = true)
    {
        var neighbor = new Neighbor()
        {
            point = climbPoint,
            direction = direction,
            connectionType = connectionType,
            isTwoWay = true
        };

        neighbors.Add(neighbor);
    }

    private void OnDrawGizmos()
    {
        foreach (var neighbor in neighbors)
        {
            Debug.DrawRay(transform.position, transform.forward, UnityEngine.Color.blue);
            if (neighbor.point != null)
                Debug.DrawLine(transform.position, neighbor.point.transform.position, neighbor.isTwoWay ? UnityEngine.Color.green : UnityEngine.Color.yellow);
        }
    }

    public Neighbor GetNeighbor(Vector2 direction)
    {
        Neighbor neighbor = null;
        if (direction.y != 0)
            neighbor = neighbors.FirstOrDefault(n => n.direction.y == direction.y);

        if (neighbor == null && direction.x != 0)
            neighbor = neighbors.FirstOrDefault(n => n.direction.x == direction.x);

        return neighbor;  
    }
}


[System.Serializable]
public class Neighbor
{
    public ClimbPoint point;
    public Vector2 direction;
    public ConnectionType connectionType;
    public bool isTwoWay = true;
}

public enum ConnectionType { Jump, Move}