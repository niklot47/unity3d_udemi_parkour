using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    [SerializeField] List<ParkourAction> parkourActions;
    [SerializeField] ParkourAction jumpDownAction;
    [SerializeField] float autoDropHeight = 1.2f;

    EnvironmentScanner environmentScanner;
    Animator animator;
    PlayerController playerController;

    void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        var hitData = environmentScanner.ObstacleCheck();

        if (Input.GetButton("Jump") && !playerController.InAction && !playerController.IsHanging)
        {

            if (hitData.forwardHitFound)
            {
                foreach (var action in parkourActions)
                {
                    if (action.checkIfPosible(hitData, transform))
                    {
                        StartCoroutine(DoParkourAktion(action));
                        break;
                    }
                }

            }
        }


        if (playerController.IsOnLedge && !playerController.InAction && !hitData.forwardHitFound && (Input.GetButton("Jump")
            ||
            playerController.LedgeData.height < autoDropHeight))
        {
            if (playerController.LedgeData.angle <= 50)
            {
                playerController.IsOnLedge = false;
                StartCoroutine(DoParkourAktion(jumpDownAction));
            }
        }
    }

    IEnumerator DoParkourAktion(ParkourAction action)
    {
        playerController.SetControl(false);
        MatchTargetParams matchTargetParams = null;
        if (action.EnableTargetMatching)
        {
            matchTargetParams = new MatchTargetParams()
            {
                pos = action.MatchPosition,
                bodyPart = action.MatchBodyPart,
                posWeight = action.MatchPosWeight,
                startTime = action.MatchStartTime,
                targetTime = action.MatchTargetTime
            };
        }
        
        yield return playerController.DoAction(action.AnimName,
                                                matchTargetParams,
                                                action.TargetRoatation,
                                                action.RotateToObstacle,
                                                action.PostActionDelay,
                                                action.Mirror);

        playerController.SetControl(true);
    }


    void MatchTarget(ParkourAction action)
    {
        if (animator.isMatchingTarget) return;

        animator.MatchTarget(action.MatchPosition,
                            transform.rotation,
                            action.MatchBodyPart,
                            new MatchTargetWeightMask(action.MatchPosWeight, 0),
                            action.MatchStartTime,
                            action.MatchTargetTime);
        
    }
}
