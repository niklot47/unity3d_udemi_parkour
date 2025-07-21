using System.Reflection;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/New parkour acrtion")]

public class ParkourAction : ScriptableObject
{
    [Header("Animation parameters")]
    [SerializeField] string animName;
    [SerializeField] string obstacleTag;

    [Header("Parkour parameters")]
    [SerializeField] float minHeight;
    [SerializeField] float maxHeight;
    [SerializeField] bool rotateToObstacle;
    [SerializeField] float postActionDelay;

    [Header("Target matching")]
    [SerializeField] bool enableTargetMatching = true;
    [SerializeField] protected AvatarTarget matchBodyPart;
    [SerializeField] float matchStartTime;
    [SerializeField] float matchTargetTime;
    [SerializeField] Vector3 matchPosWeight = new Vector3(0, 1, 0);

    public Quaternion TargetRoatation { get; set; }
    public Vector3 MatchPosition { get; set; }
    public bool Mirror { get; set; }

    public virtual bool checkIfPosible(ObstacleHitData hitData, Transform player)
    {
        if (!string.IsNullOrEmpty(obstacleTag) && hitData.forwardHit.transform.tag != obstacleTag)
            return false;

        float height = hitData.heightHit.point.y - player.position.y;
        if (height <= minHeight || height > maxHeight)
            return false;

        if (rotateToObstacle)
            TargetRoatation = Quaternion.LookRotation(-hitData.forwardHit.normal);

        if (enableTargetMatching)
            MatchPosition = hitData.heightHit.point;

        return true;
    }

    public string AnimName => animName;
    public bool RotateToObstacle => rotateToObstacle;
    public bool EnableTargetMatching => enableTargetMatching;
    public AvatarTarget MatchBodyPart => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;
    public Vector3 MatchPosWeight => matchPosWeight;
    public float PostActionDelay => postActionDelay;
}
