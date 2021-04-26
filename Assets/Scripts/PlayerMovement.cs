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

    enum State { Jump, Fall, Dash, Pirouette, Idle, Move };
    State state;


    private void Awake()
    {
        state = State.Idle;
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (controller.isGrounded && state != State.Dash)
            {
                jumpVector.y = Mathf.Sqrt(-2 * jumpHeight * gravity);
                state = State.Jump;
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse1) && state != State.Dash && state != State.Fall)
        {
            dashTimer = 0;
            dashAngle = lastAngle;
            state = State.Dash;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
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

        if (state != State.Dash)
        {
            if (rawDirection.magnitude > deadZone)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                lastAngle = angle;
                if (state != State.Pirouette)
                {
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);
                }

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                rawInput = (Mathf.Clamp((Mathf.Abs(horizontal) + Mathf.Abs(vertical)), 0, 1));
                outputVelocity = rawInput * speed;
                outputDirection = moveDir;

                if (controller.isGrounded)
                {
                    state = State.Move;
                }
            }
            else { outputVelocity = Mathf.Lerp(speed, 0, (inertiaTimer / inertiaTime)); }
        }
        if (state == State.Dash)
        {
            jumpVector.y = 0;
            transform.rotation = Quaternion.Euler(0f, dashAngle, 0f);
            outputDirection = controller.transform.forward;
            outputVelocity = speed + Mathf.Lerp(dashSpeed, 0, dashTimer / dashTime);
            dashTimer += Time.deltaTime;
            if(dashTimer > dashTime)
            {
                state = State.Fall;
            }
        }
        
        #endregion

        //dash collisions

        RaycastHit dashCollide;
        float collideDetect = controller.radius * 1.75f;
        if (Physics.SphereCast(dashForward.transform.position, controller.radius/2, dashForward.transform.forward, out dashCollide, collideDetect))
        {
            if (state == State.Dash)
            {
                Enemy enemy = dashCollide.collider.gameObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Vector3 knockbackDir = (enemy.transform.position - controller.transform.position).normalized;
                    enemy.Knockback(knockbackDir, speed, 0.5f);
                    inertiaDirection = (controller.transform.position - enemy.transform.position);
                    state = State.Pirouette;
                }
                else
                {
                    inertiaDirection = Vector3.Reflect(dashForward.transform.forward, dashCollide.normal);
                    state = State.Pirouette;
                }

                GameObject impact = Instantiate(hitEffect, dashCollide.point, Quaternion.LookRotation(dashCollide.normal));
                Destroy(impact, 2f);

                inertiaTimer = 0;
                jumpVector.y = Mathf.Sqrt(-2 * jumpHeight * gravity);
                inertiaTimer = 0f;
            }
        }

        //inertia
        Vector3 finalDirection = Vector3.Lerp(inertiaDirection, outputDirection, (inertiaTimer / inertiaTime));

        controller.Move(finalDirection * outputVelocity * Time.deltaTime);
        controller.Move(jumpVector * Time.deltaTime);

        #region Basic Jump
        if (state != State.Dash)
        {
            if (!controller.isGrounded)
            {
                jumpVector.y += gravity * Time.deltaTime;
            }
            if (controller.isGrounded)
            {
                jumpVector.y = gravity * Time.deltaTime;
            }
        }
        #endregion

        //timer iteration
        inertiaTimer += Time.deltaTime;

        Debug.Log(state);
        
    }
}
