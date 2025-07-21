using System;
using System.Collections;
using UnityEngine;

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
                    currentPoint = ledgeHit.transform.GetComponent<ClimbPoint>();
                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("IdleToHang", ledgeHit.transform, 0.4f, 0.533f));
                }
            }
        }
        else
        {
            float horizontal = Mathf.Round(Input.GetAxis("Horizontal"));
            float vertical = Mathf.Round(Input.GetAxis("Vertical"));
            var input = new Vector2(horizontal, vertical);

            if (playerController.InAction || input == Vector2.zero) return;

            var neighbor = currentPoint.GetNeighbor(input);

            if (neighbor == null) return;

            if (neighbor.connectionType == ConnectionType.Jump && Input.GetButton("Jump"))
            {
                currentPoint = neighbor.point;
                
                if (neighbor.direction.y == 1)
                    StartCoroutine(JumpToLedge("HangHopUp", currentPoint.transform, 0.44f, 0.6f));

                if (neighbor.direction.y == -1)
                    StartCoroutine(JumpToLedge("HangHopDown", currentPoint.transform, 0.31f, 0.65f));

                if (neighbor.direction.x == 1)
                    StartCoroutine(JumpToLedge("HangHopRight", currentPoint.transform, 0.24f, 0.36f));
                    
                if (neighbor.direction.x == -1)
                    StartCoroutine(JumpToLedge("HangHopLeft", currentPoint.transform, 0.32f, 0.45f));
            }
        }
    }

    IEnumerator JumpToLedge(string animName, Transform ledge, float matchStartTime, float matchTargetTime, AvatarTarget avatarTarget = AvatarTarget.RightHand)
    {
        var matchParam = new MatchTargetParams()
        {
            pos = GetHandPos(ledge),
            bodyPart = avatarTarget,
            posWeight = Vector3.one,
            startTime = matchStartTime,
            targetTime = matchTargetTime
        };

        var targetRotation = Quaternion.LookRotation(-ledge.forward);

        yield return playerController.DoAction(animName, matchParam, targetRotation, true);

        playerController.IsHanging = true;
    }

    Vector3 GetHandPos(Transform ledge)
    {
        return ledge.position + ledge.forward * 0.1f + Vector3.up * 0.1f - ledge.right * 0.25f;
    }
}
