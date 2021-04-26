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

    Vector3 moveDir;
    Vector3 outputDir;
    Vector3 vertDir;

    void Awake()
    {
        
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        _body.Move(outputDir * Time.deltaTime);
        _body.Move(vertDir * Time.deltaTime);
        Debug.Log(_body.isGrounded);
        vertDir.y -= _gravity * Time.deltaTime;

        if (!_body.isGrounded)
        {
            bounceVelocity = -vertDir.y * _bounceDecay;
        }
        else
        {
            vertDir.y = bounceVelocity;
        }
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
}
