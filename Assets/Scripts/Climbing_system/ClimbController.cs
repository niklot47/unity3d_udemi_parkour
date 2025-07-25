using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class ClimbControllers : MonoBehaviour
{
    PlayerController playerController;
    EnvironmentScanner environmentScanner;

    ClimbPoint currentPoint;

    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (!playerController.IsHanging)
        {
            if (Input.GetButton("Jump") && !playerController.InAction)
            {
                if (environmentScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit))
                {
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);
                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("IdleToHang", currentPoint.transform, 0.4f, 0.533f));
                }
            }

            if (Input.GetButton("Drop") && !playerController.InAction)
            {
                if (environmentScanner.DropLedgeCheck(out RaycastHit ledgeHit))
                {
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);

                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("DropToHang", currentPoint.transform, 0.3f, 0.45f, handOffset : new Vector3(0.25f, 0.2f, -0.2f)));
                }
            }
        }
        else
        {
            if (Input.GetButton("Drop") && !playerController.InAction)
            {
                StartCoroutine(JumpFromWall());
                return;
            }

            float horizontal = Mathf.Round(Input.GetAxis("Horizontal"));
            float vertical = Mathf.Round(Input.GetAxis("Vertical"));
            var input = new Vector2(horizontal, vertical);

            if (playerController.InAction || input == Vector2.zero) return;

            if (currentPoint.MountPoint && input.y == 1 && Input.GetButton("Jump"))
            {
                StartCoroutine(MountFromHand());
                return;
            }

            var neighbor = currentPoint.GetNeighbor(input);

            if (neighbor == null) return;

            if (neighbor.connectionType == ConnectionType.Jump && Input.GetButton("Jump"))
            {
                currentPoint = neighbor.point;

                if (neighbor.direction.y == 1)
                    StartCoroutine(JumpToLedge("HangHopUp", currentPoint.transform, 0.44f, 0.6f, handOffset: new Vector3(0.25f, 0.1f, 0.15f)));

                if (neighbor.direction.y == -1)
                    StartCoroutine(JumpToLedge("HangHopDown", currentPoint.transform, 0.46f, 0.63f, AvatarTarget.LeftHand, handOffset: new Vector3(0.29f, -0.2f, 0.05f)));

                if (neighbor.direction.x == 1)
                    StartCoroutine(JumpToLedge("HangHopRight", currentPoint.transform, 0.24f, 0.36f, handOffset: new Vector3(0.25f, 0.12f, 0.15f)));

                if (neighbor.direction.x == -1)
                    StartCoroutine(JumpToLedge("HangHopLeft", currentPoint.transform, 0.32f, 0.45f));
            }
            else if (neighbor.connectionType == ConnectionType.Move)
            {
                currentPoint = neighbor.point;

                if (neighbor.direction.x == 1)
                    StartCoroutine(JumpToLedge("ShimmyRight", currentPoint.transform, 0f, 0.38f, handOffset: new Vector3(0.25f, 0.05f, 0.1f)));

                if (neighbor.direction.x == -1)
                    StartCoroutine(JumpToLedge("ShimmyLeft", currentPoint.transform, 0f, 0.38f, AvatarTarget.LeftHand, handOffset: new Vector3(0.25f, 0.05f, 0.1f)));
            }
        }
    }

    IEnumerator JumpToLedge(string animName,
                            Transform ledge,
                            float matchStartTime,
                            float matchTargetTime,
                            AvatarTarget avatarTarget = AvatarTarget.RightHand,
                            Vector3? handOffset = null)
    {
        var matchParam = new MatchTargetParams()
        {
            pos = GetHandPos(ledge, avatarTarget, handOffset),
            bodyPart = avatarTarget,
            posWeight = Vector3.one,
            startTime = matchStartTime,
            targetTime = matchTargetTime
        };

        var targetRotation = Quaternion.LookRotation(-ledge.forward);

        yield return playerController.DoAction(animName, matchParam, targetRotation, true);

        playerController.IsHanging = true;
    }

    Vector3 GetHandPos(Transform ledge, AvatarTarget hand, Vector3? handOffset = null)
    {
        Vector3 offsetValue = (handOffset != null) ? handOffset.Value : new Vector3(0.25f, 0.1f, 0.1f);

        var hDit = hand == AvatarTarget.RightHand ? ledge.right : -ledge.right;
        return ledge.position + ledge.forward * offsetValue.z + Vector3.up * offsetValue.y - hDit * offsetValue.x;
    }

    IEnumerator JumpFromWall()
    {
        playerController.IsHanging = false;
        yield return playerController.DoAction("JumpFromWall");

        playerController.ResetTaergetRotation();
        playerController.SetControl(true);
    }

    IEnumerator MountFromHand()
    {
        playerController.IsHanging = false;
        yield return playerController.DoAction("HangToCrouch");
        playerController.EnableCharacterController(true);

        yield return new WaitForSeconds(0.5f);

        playerController.ResetTaergetRotation();
        playerController.SetControl(true);
    }

    ClimbPoint GetNearestClimbPoint(Transform ledge, Vector3 hitPoint)
    {
        var points = ledge.GetComponentsInChildren<ClimbPoint>();

        ClimbPoint nearestPoint = null;
        float nearestPointdistance = Mathf.Infinity;

        foreach (var point in points)
        {
            float distance = Vector3.Distance(point.transform.position, hitPoint);

            if (distance < nearestPointdistance)
            {
                nearestPointdistance = distance;
                nearestPoint = point;
            }
        }

        return nearestPoint;
    }
}
