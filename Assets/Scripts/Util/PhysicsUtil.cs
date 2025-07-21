using System.Collections.Generic;
using UnityEngine;

public class PhysicsUtil 
{
    public static bool ThreRaycast(Vector3 origin,
                                    Vector3 dir,
                                    float spacing,
                                    Transform transform,
                                    out List<RaycastHit> hits,
                                    float rayLength,
                                    LayerMask layer,
                                    bool DebugDraw = false)
    {
        bool centerHitFound = Physics.Raycast(origin, Vector3.down, out RaycastHit centerHit, rayLength, layer);
        bool leftHitFound = Physics.Raycast(origin - transform.right * spacing, Vector3.down, out RaycastHit leftHit, rayLength, layer);
        bool rightHitFound = Physics.Raycast(origin + transform.right * spacing, Vector3.down, out RaycastHit rightHit, rayLength, layer);
        
        hits = new List<RaycastHit>() {centerHit, leftHit, rightHit };

        bool hitFound = centerHitFound || leftHitFound || rightHitFound;

        if (DebugDraw && hitFound)
        { 
            Debug.DrawLine(origin, centerHit.point, Color.cyan );
            Debug.DrawLine(origin - transform.right * spacing, leftHit.point, Color.cyan);
            Debug.DrawLine(origin + transform.right * spacing, rightHit.point, Color.cyan);
        }

        return hitFound;
    }
}
