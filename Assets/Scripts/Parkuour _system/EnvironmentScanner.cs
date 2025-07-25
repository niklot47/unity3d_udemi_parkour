using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [Header("Forward Raycast")]
    [SerializeField] Vector3 forwardRayOffset = new Vector3(0, 0.25f, 0);
    [SerializeField] float forwardRayLength = 0.8f;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] float heightRayLenght = 5;
    [Header("Ledge Raycast")]
    [SerializeField] float ledgeOriginOffset = 0.5f;
    [SerializeField] float ledgeRayLength = 10;
    [SerializeField] float ledgeHightThrashold = 0.75f;
    [SerializeField] float ladgeThreReyOffset = 0.25f;
    [Header("Climbing Raycast")]
    [SerializeField] int climbRaycastNumb = 15;
    [SerializeField] float firstRaycastHeight = 1.5f;
    [SerializeField] float climbRaycastGap = 0.18f;
    [SerializeField] float climbLedgeRayLength = 1.5f;
    [SerializeField] LayerMask climbObstacleLayer;
    [Header("Drop Ledge Raycast")]
    [SerializeField] float dropLedgeDownOfset = 0.1f;
    [SerializeField] float dropLedgeForwardOfset = 4f;
    [SerializeField] float dropLedgeRayLength = 3f;
    

    public ObstacleHitData ObstacleCheck(bool debug = false)
    {
        var hitData = new ObstacleHitData();
        var forwardOrigin = transform.position + forwardRayOffset;

        hitData.forwardHitFound = Physics.Raycast(forwardOrigin,
                                        transform.forward,
                                        out hitData.forwardHit,
                                        forwardRayLength,
                                        obstacleLayer);
        if (debug)
        {
            Debug.DrawRay(forwardOrigin,
                          transform.forward * forwardRayLength,
                          (hitData.forwardHitFound) ? Color.red : Color.white);
        }

        if (hitData.forwardHitFound)
        {
            var heightOrigin = hitData.forwardHit.point + Vector3.up * heightRayLenght;
            hitData.heightHitFound = Physics.Raycast(heightOrigin,
                                                    Vector3.down,
                                                    out hitData.heightHit,
                                                    heightRayLenght,
                                                    obstacleLayer);
            if (debug)
            {
                Debug.DrawRay(heightOrigin,
                        Vector3.down * heightRayLenght,
                        (hitData.heightHitFound) ? Color.red : Color.white);
            }
        }

        return hitData;
    }

    public bool ObstacleLedgeCheck(Vector3 moveDir, out LedgeData ledgeData)
    {
        ledgeData = new LedgeData();

        if (moveDir == Vector3.zero)
            return false;

        var origin = transform.position + moveDir * ledgeOriginOffset + Vector3.up;

        if (PhysicsUtil.ThreRaycast(origin, Vector3.down, ladgeThreReyOffset, transform, out List<RaycastHit> hits, ledgeRayLength, obstacleLayer, true))
        {

            var validHits = hits.Where(h => transform.position.y - h.point.y > ledgeHightThrashold).ToList();

            if (validHits.Count > 0)
            {
                var surfaceRayOrigin = validHits[0].point;
                surfaceRayOrigin.y = transform.position.y - 0.1f;

                if (Physics.Raycast(surfaceRayOrigin, transform.position - surfaceRayOrigin, out RaycastHit surfaceHit, 2, obstacleLayer))
                {
                    Debug.DrawLine(surfaceRayOrigin, transform.position, Color.yellow);
                    float height = transform.position.y - validHits[0].point.y;

                    ledgeData.angle = Vector3.Angle(transform.forward, surfaceHit.normal);
                    ledgeData.height = height;
                    ledgeData.surfaceHit = surfaceHit;

                    return true;
                }
            }
        }

        return false;
    }

    public bool ClimbLedgeCheck(Vector3 moveDir, out RaycastHit ledgeHit)
    {
        ledgeHit = new RaycastHit();

        if (moveDir == Vector3.zero)
            return false;
            
        var origin = transform.position + Vector3.up * firstRaycastHeight;
        var offset = new Vector3(0, climbRaycastGap, 0);
        bool hited = false;
        for (int i = 0; i < climbRaycastNumb; i++)
        {
            if (Physics.Raycast(origin + i * offset, moveDir, out RaycastHit hit, climbLedgeRayLength, climbObstacleLayer))
            {
                if (!hited)
                { 
                    ledgeHit = hit;
                    hited = true;
                }
                Debug.DrawRay(origin + i * offset, moveDir, Color.green);
            }
            else
            {
                Debug.DrawRay(origin + i * offset, moveDir, Color.red);
            }
        }
        return hited;        

    }

    public bool DropLedgeCheck(out RaycastHit ledgeHit)
    {
        bool hited = false;
        ledgeHit = new RaycastHit();

        var origin = transform.position + Vector3.down * 0.1f + transform.forward * 2f;
        if (Physics.Raycast(origin, -transform.forward, out RaycastHit hit, dropLedgeRayLength, climbObstacleLayer))
        {
            ledgeHit = hit;
            hited = true;
        }
        Debug.DrawRay(origin,  -transform.forward, hited? Color.yellow: Color.grey);
        return hited;
    }
}

public struct ObstacleHitData
{
    public bool forwardHitFound;
    public bool heightHitFound;
    public RaycastHit forwardHit;
    public RaycastHit heightHit;
}

public struct LedgeData
{
    public float height;
    public float angle;
    public RaycastHit surfaceHit;
}