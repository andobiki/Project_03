using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public GameObject art;
    public GameObject hitEffect;
    public Transform cam;
    public Transform dashForward;
    public Transform pirouettePoint;
    public Animator anim;

    public float hitstopTime;
    public float speed;
    public float jumpHeight;
    public float turnSmoothTime;
    public float deadZone;
    public float minVelocity;
    public float gravity;
    public float dashSpeed;
    public float dashTime;
    public float inertiaTime;
    public float impactGraceTime;
    public int revTime, impactStopTime;

    float rawInput;
    Vector3 outputDirection;
    float lastAngle;
    float outputVelocity;
    float turnSmoothVelocity;
    Vector3 jumpVector;
    float dashTimer;
    float dashAngle;
    Vector3 inertiaDirection;
    float inertiaTimer;
    int revTimer, impactTimer;
    bool canDash;
    bool hasLanded;
    float impactGraceTimer;

    enum State { Jump, Fall, Dash, Pirouette, PirouetteFall, Move };
    State state;


    private void Awake()
    {
        state = State.Move;
        revTimer = revTime;
        impactTimer = impactStopTime;
        canDash = true;
        hasLanded = false;
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (controller.isGrounded && state != State.Dash)
            {
                jumpVector.y = Mathf.Sqrt(-2 * jumpHeight * gravity);
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
                {
                    anim.SetTrigger("Jump");
                }
                hasLanded = false;
                state = State.Jump;
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse1) && canDash)
        {
            dashTimer = 0;
            revTimer = 0;
            dashAngle = lastAngle;
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("ShoulderSmash"))
            {
                anim.SetTrigger("ShoulderSmash");
            }
            impactGraceTimer = 0;
            canDash = false;
            state = State.Dash;
        }
    }

    void FixedUpdate()
    {
        #region Horizontal Movement
        //basic horizontal movement code
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 rawDirection = new Vector3(horizontal, 0f, vertical);
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        float moveVelocity = rawDirection.magnitude;
        if (revTimer > revTime && impactTimer > impactStopTime)
        {
            if (state != State.Dash)
            {
                if (rawDirection.magnitude > deadZone)
                {
                    if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Run") && state == State.Move)
                    {
                        anim.SetTrigger("Run");
                    }
                    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                    lastAngle = angle;
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                    rawInput = (Mathf.Clamp((Mathf.Abs(horizontal) + Mathf.Abs(vertical)), 0, 1));
                    outputVelocity = rawInput * speed;
                    outputDirection = moveDir;

                    if (controller.isGrounded && state != State.Jump && state != State.Pirouette && canDash)
                    {
                        state = State.Move;
                    }
                }
                else
                {
                    outputVelocity = Mathf.Lerp(speed, 0, (inertiaTimer / inertiaTime));
                    if (state == State.Move && !anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Land"))
                    {
                        anim.SetTrigger("Idle");
                    }
                }
            }
            if (state == State.Dash)
            {
                jumpVector.y = 0;
                transform.rotation = Quaternion.Euler(0f, dashAngle, 0f);
                outputDirection = controller.transform.forward;
                outputVelocity = speed + Mathf.Lerp(dashSpeed, 0, dashTimer / dashTime);
                dashTimer += Time.deltaTime;
                if (dashTimer > dashTime)
                {
                    state = State.Fall;
                    anim.SetTrigger("Fall");
                }
            }
        }


        #endregion

        //dash collisions

        #region Dash Collisions
        RaycastHit dashCollide;
        float collideDetect = controller.radius * 1.75f;
        if (Physics.SphereCast(dashForward.transform.position, controller.radius / 2, dashForward.transform.forward, out dashCollide, collideDetect))
        {
            if (state == State.Dash)
            {
                Enemy enemy = dashCollide.collider.gameObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Vector3 knockbackDir = (enemy.transform.position - controller.transform.position).normalized;
                    enemy.Knockback(knockbackDir, speed, 0.5f);
                    inertiaDirection = (controller.transform.position - enemy.transform.position);
                    anim.SetTrigger("Impact");
                    impactTimer = 0;
                    hasLanded = false;
                    state = State.Pirouette;
                }
                else
                {
                    inertiaDirection = Vector3.Reflect(dashForward.transform.forward, dashCollide.normal);
                    anim.SetTrigger("Impact");
                    impactTimer = 0;
                    hasLanded = false;
                    state = State.Pirouette;
                }
                

                GameObject impact = Instantiate(hitEffect, dashCollide.point, Quaternion.LookRotation(dashCollide.normal));
                Destroy(impact, 2f);

                inertiaTimer = 0;
                jumpVector.y = Mathf.Sqrt(-2 * jumpHeight * gravity);
                inertiaTimer = 0f;
            }
        }
        if (state == State.Pirouette || state == State.PirouetteFall)
        {
            RaycastHit jumpCollide;
            if (Physics.SphereCast(pirouettePoint.transform.position, controller.radius * 1.75f, Vector3.down, out jumpCollide, 0.5f))
            {
                Enemy enemy = jumpCollide.collider.gameObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    if (enemy.canBeBounced == true)
                    {
                        GameObject impact = Instantiate(hitEffect, jumpCollide.point, Quaternion.LookRotation(dashCollide.normal));
                        Destroy(impact, 2f);
                        jumpVector.y = Mathf.Sqrt(-2 * jumpHeight * gravity);
                        impactTimer = 0;
                        state = State.Pirouette;
                        enemy.Bounce();
                        FindObjectOfType<AudioManager>().Play("dashImpact");
                        hasLanded = false;
                        canDash = true;
                    }
                }
            }
        }
        
        #endregion

        //inertia
        Vector3 finalDirection = Vector3.Lerp(inertiaDirection, outputDirection, (inertiaTimer / inertiaTime));

        if (revTimer > revTime && impactTimer > impactStopTime)
        {
            controller.Move(finalDirection * outputVelocity * Time.deltaTime);
            controller.Move(jumpVector * Time.deltaTime);
        }

        #region Basic Jump
        if (state != State.Dash)
        {
            if (revTimer > revTime && impactTimer > impactStopTime)
            {
                if (!controller.isGrounded)
                {
                    jumpVector.y += gravity * Time.deltaTime;
                }
                if (controller.isGrounded)
                {
                    jumpVector.y = gravity * Time.deltaTime;
                    canDash = true;
                }
            }

            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Fall") && state != State.Fall && jumpVector.y < -0.2 && !controller.isGrounded)
            {
                if (state == State.Jump)
                {
                    anim.SetTrigger("Fall");
                    state = State.Fall;
                }
                if (state == State.Pirouette)
                {
                    state = State.PirouetteFall;
                }
            }
            if (state == State.Fall || state == State.Pirouette || state == State.PirouetteFall)
            {
                if (controller.isGrounded && impactGraceTimer > impactGraceTime && hasLanded == false)
                {
                    anim.SetTrigger("Land");
                    hasLanded = true;
                }
            }

        }
        #endregion

        

        Debug.Log(state);

        //timer iteration
        inertiaTimer += Time.deltaTime;
        impactTimer++;
        revTimer++;
        impactGraceTimer += Time.deltaTime;
    }
}
