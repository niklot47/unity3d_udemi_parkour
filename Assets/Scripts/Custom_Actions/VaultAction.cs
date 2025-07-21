using UnityEngine;


[CreateAssetMenu(menuName = "Parkour System/Custom Actions/New vault actions")]

public class VaultAction : ParkourAction
{
    public override bool checkIfPosible(ObstacleHitData hitData, Transform player)
    {
        if (!base.checkIfPosible(hitData, player))
            return false;

        var hitPoin = hitData.forwardHit.transform.InverseTransformPoint(hitData.forwardHit.point);
        if (hitPoin.z < 0 && hitPoin.x < 0 || hitPoin.z > 0 && hitPoin.x > 0)
        {
            Mirror = true;
            matchBodyPart = AvatarTarget.RightHand;
        }
        else
        {
            Mirror = false;
            matchBodyPart = AvatarTarget.LeftHand;
        }

        return true;
    }
}
