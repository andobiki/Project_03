using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum _type { small, big };
    public CharacterController _body;
    public float _gravity;
    public float _knockbackTime;
    public float _bounceDecay;

    float knockbackTimer;
    float bounceVelocity;

    public float topBounceTime;
    float topBounceTimer;
    public bool canBeBounced;

    Vector3 moveDir;
    Vector3 outputDir;
    Vector3 vertDir;


    void Awake()
    {
        canBeBounced = true;
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        _body.Move(outputDir * Time.deltaTime);
        _body.Move(vertDir * Time.deltaTime);
        vertDir.y -= _gravity * Time.deltaTime;

        if (!_body.isGrounded)
        {
            bounceVelocity = -vertDir.y * _bounceDecay;
        }
        else
        {
            vertDir.y = bounceVelocity;
        }

        if (topBounceTimer > topBounceTime)
        {
            canBeBounced = true;
        }

        topBounceTimer += Time.deltaTime;
    }

    private void LateUpdate()
    {
        knockbackTimer += Time.deltaTime;
        outputDir = Vector3.Lerp(moveDir, Vector3.zero, (knockbackTimer / _knockbackTime));
    }

    public void Knockback(Vector3 direction, float _velocity, float angle) {
        moveDir = direction * _velocity;
        vertDir.y = _velocity * angle;
        knockbackTimer = 0;
    }

    public void Bounce()
    {
        if (canBeBounced)
        {
            topBounceTimer = 0;
            canBeBounced = false;
        }
    }
}
