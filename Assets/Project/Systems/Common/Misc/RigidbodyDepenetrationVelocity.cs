using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class RigidbodyDepenetrationVelocity : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField, OnValueChanged(nameof(SetDepVelocity)), Min(0)] private float maxVel;

    public float Velocity
    {
        get => maxVel;
        set
        {
            maxVel = value;
            SetDepVelocity();
        }
    }
    
    public void SetDepVelocity()
    {
        maxVel = Mathf.Clamp(maxVel, 0f, Mathf.Infinity);
        if(rb != null)
            rb.maxDepenetrationVelocity = maxVel;
    }

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        SetDepVelocity();
    }
}
