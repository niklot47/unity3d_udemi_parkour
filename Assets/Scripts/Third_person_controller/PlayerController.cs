using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Speed variables")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 50f;
    [Header("Ground Checker variables")]
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    bool hasControl = true;
    float ySpeed;
    bool isGrounded = false;
    public bool IsOnLedge { get; set; }
    public LedgeData LedgeData { get; set; }
    Vector3 moveDirection;
    Vector3 desiredDirection;
    Vector3 velocity;
    public bool InAction { get; private set; }
    public bool IsHanging { get; set; }

    Quaternion targetRotation;
    CameraController cameraController;
    Animator animator;
    CharacterController characterController;
    EnvironmentScanner environmentScanner;

    void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));
        var moveInput = new Vector3(0, 0, 0);
        moveInput = new Vector3(h, 0, v).normalized;
        desiredDirection = cameraController.PlanerRotation * moveInput;
        moveDirection = desiredDirection;
        velocity = Vector3.zero;

        if (!hasControl) return;
        if (IsHanging) return;

        GroundCheck();
        animator.SetBool("isGround", isGrounded);
        if (isGrounded)
        {
            IsOnLedge = environmentScanner.ObstacleLedgeCheck(desiredDirection, out LedgeData ledgeData);
            ySpeed = -0.5f;
            velocity = desiredDirection * moveSpeed;

            if (IsOnLedge)
            {
                LedgeData = ledgeData;
                LedgeMovement();
            }

            animator.SetFloat("moveAmount", velocity.magnitude / moveSpeed, 0.2f, Time.deltaTime);
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
            velocity = transform.forward * moveSpeed / 2;
            animator.SetFloat("moveAmount", -1, 0.2f, Time.deltaTime);
        }

        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0 && moveDirection.magnitude > 0.2f && isGrounded)
        {
            targetRotation = Quaternion.LookRotation(moveDirection);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    void LedgeMovement()
    {
        float signedAngle = Vector3.SignedAngle(LedgeData.surfaceHit.normal, desiredDirection, Vector3.up);
        float angle = Mathf.Abs(signedAngle);

        if (Vector3.Angle(desiredDirection, transform.forward) >= 80)
        {
            velocity = Vector3.zero;
            return;
        }

        if (angle < 50)
        {
            velocity = Vector3.zero;
            moveDirection = Vector3.zero;
        }
        else if (angle < 90)
        {
            var left = Vector3.Cross(Vector3.up, LedgeData.surfaceHit.normal);
            var dir = left * Mathf.Sign(signedAngle);

            velocity = velocity.magnitude * dir;
            moveDirection = dir;
        }
    }

    public IEnumerator DoAction(string animName,
                        MatchTargetParams matchTargetParams,
                        Quaternion targetRotation,
                        bool rotate = false,
                        float postActionDelay = 0f,
                        bool mirror = false)
    {
        InAction = true;

        animator.SetBool("mirrorAction", mirror);
        animator.CrossFade(animName, 0.2f);
        yield return null;


        var animationState = animator.GetNextAnimatorStateInfo(0);
        if (!animationState.IsName(animName))
            Debug.LogError("Animation is wrong! " + animName);

        float timer = 0f;
        while (timer <= animationState.length)
        {
            timer += Time.deltaTime;

            if (rotate)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (matchTargetParams != null)
                MatchTarget(matchTargetParams);

            if (animator.IsInTransition(0) && timer > 0.5f)
                break;

            yield return null;
        }

        yield return new WaitForSeconds(postActionDelay);

        InAction = false;
    }

    void MatchTarget(MatchTargetParams matchTargetParams)
    {
        if (animator.isMatchingTarget) return;

        animator.MatchTarget(matchTargetParams.pos,
                            transform.rotation,
                            matchTargetParams.bodyPart,
                            new MatchTargetWeightMask(matchTargetParams.posWeight, 0),
                            matchTargetParams.startTime,
                            matchTargetParams.targetTime);

    }

    public void SetControl(bool hasControl)
    {
        this.hasControl = hasControl;
        characterController.enabled = hasControl;

        if (hasControl)
        {
            animator.SetFloat("moveAmount", 0f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }

    public float RotationSpeed => rotationSpeed;
    public bool HasControl { get => hasControl; set => hasControl = value; }
}

public class MatchTargetParams
{
    public Vector3 pos;
    public AvatarTarget bodyPart;
    public Vector3 posWeight;
    public float startTime;
    public float targetTime;
}
