using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveForce     = 26f;
    public float maxSpeed      = 7f;
    public float moveDrag      = 7f;
    public float rotationSpeed = 8f;

    [Header("Jump")]
    public float jumpForce       = 7f;
    public float groundCheckDist = 1.6f;

    [Header("Step Up")]
    public float stepHeight = 0.4f;  // auto step lên bậc thấp (tile cao 0.35)

    [Header("References")]
    public FloatingJoystick joystick;
    public Transform        cameraTransform;

    private Rigidbody rb;
    private Animator  anim;
    private bool      isGrounded;
    private bool      jumpRequested;
    private float     smoothSpeed = 0f;

    private static readonly int HashSpeed      = Animator.StringToHash("Speed");
    private static readonly int HashIsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int HashJump       = Animator.StringToHash("Jump");

    void Awake()
    {
        rb   = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        rb.mass               = 2f;
        rb.drag               = moveDrag;
        rb.angularDrag        = 0.5f;
        rb.freezeRotation     = true;
        rb.useGravity         = true;
        rb.interpolation      = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
        if (anim != null) anim.applyRootMotion = false;
    }

    void Update()
    {
        isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f, Vector3.down,
            groundCheckDist + 0.1f, ~0, QueryTriggerInteraction.Ignore);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) RequestJump();
        UpdateAnim();
    }

    void FixedUpdate() { Move(); DoJump(); }

    void Move()
    {
        float h = (joystick != null) ? joystick.Horizontal : 0f;
        float v = (joystick != null) ? joystick.Vertical   : 0f;
        h += Input.GetAxisRaw("Horizontal"); v += Input.GetAxisRaw("Vertical");
        h = Mathf.Clamp(h, -1f, 1f); v = Mathf.Clamp(v, -1f, 1f);

        Vector3 camF = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        Vector3 camR = cameraTransform != null ? cameraTransform.right   : Vector3.right;
        camF.y = 0f; camF.Normalize(); camR.y = 0f; camR.Normalize();

        Vector3 dir = camF * v + camR * h;
        bool moving = dir.sqrMagnitude > 0.01f;

        if (moving)
        {
            dir.Normalize();
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude < maxSpeed)
                rb.AddForce(dir * moveForce, ForceMode.Force);

            Quaternion goal = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, goal, rotationSpeed * Time.fixedDeltaTime);

            // Auto step up — nhẹ nhàng đẩy lên khi gặp bậc thấp
            AutoStepUp(dir);
        }

        rb.drag = moving ? 1.5f : moveDrag;
    }

    void AutoStepUp(Vector3 moveDir)
    {
        // Raycast phía trước ở mức chân: nếu có vật cản thấp → đẩy lên
        Vector3 feetPos = transform.position + Vector3.up * 0.05f;
        if (Physics.Raycast(feetPos, moveDir, out RaycastHit hitLow, 0.6f, ~0, QueryTriggerInteraction.Ignore))
        {
            // Kiểm tra phía trên bậc: nếu trống → có thể bước lên
            Vector3 stepCheckPos = feetPos + Vector3.up * stepHeight;
            if (!Physics.Raycast(stepCheckPos, moveDir, 0.6f, ~0, QueryTriggerInteraction.Ignore))
            {
                // Đẩy nhẹ lên trên
                rb.AddForce(Vector3.up * 8f, ForceMode.Force);
            }
        }
    }

    void RequestJump()
    {
        jumpRequested = true;
        if (anim != null) anim.SetTrigger(HashJump);
    }

    void DoJump()
    {
        if (!jumpRequested || !isGrounded) return;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpRequested = false;
    }

    void UpdateAnim()
    {
        if (anim == null) return;
        float actualSpeed = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;
        float hi = (joystick != null) ? joystick.Horizontal : 0f;
        float vi = (joystick != null) ? joystick.Vertical   : 0f;
        hi += Input.GetAxisRaw("Horizontal"); vi += Input.GetAxisRaw("Vertical");
        bool hasInput = (hi * hi + vi * vi) > 0.01f;
        smoothSpeed = Mathf.Lerp(smoothSpeed, hasInput ? actualSpeed : 0f,
            Time.deltaTime * (hasInput ? 8f : 4f));
        anim.SetFloat(HashSpeed, smoothSpeed);
        anim.SetBool(HashIsGrounded, isGrounded);
    }

    public void OnJumpButtonPressed() { if (isGrounded) RequestJump(); }
}
