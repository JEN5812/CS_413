using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using UnityEngine;

public class Boid : MonoBehaviour
{
    private Rigidbody rigid;
    private Neighborhood neighborhood;

    [Header("Proximity Sensor")]
    public float sensorRadius = 5f;
    public LayerMask obstacleLayer;
    public float avoidanceForceStrength = 10f;

    // Start is called before the first frame update
    void Awake()
    {
        neighborhood = GetComponent<Neighborhood>();
        rigid = GetComponent<Rigidbody>();
        vel = Random.onUnitSphere * Spawner.SETTINGS.velocity;

        LookAhead();
        Colorize();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        BoidSettings bSet = Spawner.SETTINGS;
        Vector3 sumVel = Vector3.zero;

        Vector3 delta = Attractor.POS - pos;

        if (delta.magnitude > bSet.attractPull)
        {
            sumVel += delta.normalized * bSet.attractPull;
        }
        else
        {
            sumVel -= delta.normalized * bSet.attractPush;
        }

        // Collision avoidance using proximity sensor
        Vector3 avoidanceForce = Vector3.zero;

        Collider[] colliders = Physics.OverlapSphere(transform.position, sensorRadius, obstacleLayer);

        foreach (Collider collider in colliders)
        {
            Vector3 avoidanceDirection = transform.position - collider.transform.position;
            avoidanceForce += avoidanceDirection.normalized * avoidanceForceStrength;
        }

        Vector3 tooNearPos = neighborhood.avgNearPos;
        if (tooNearPos != Vector3.zero)
        {
            Vector3 velAvoid = pos - tooNearPos;
            velAvoid.Normalize();
            sumVel += velAvoid * bSet.nearAvoid;
        }

        // Velocity Matching
        Vector3 velAlign = neighborhood.avgVel;
        if (velAlign != Vector3.zero)
        {
            velAlign.Normalize();
            sumVel += velAlign * bSet.velMatching;
        }

        // Flock Centering
        Vector3 velCenter = neighborhood.avgPos;
        if (velCenter != Vector3.zero)
        {
            velCenter -= transform.position;
            velCenter.Normalize();
            sumVel += velCenter * bSet.flockCentering;
        }

        sumVel += avoidanceForce;

        sumVel.Normalize();
        vel = Vector3.Lerp(vel.normalized, sumVel, bSet.velocityEasing);
        vel *= bSet.velocity;

        LookAhead();
    }

    public Vector3 vel
    {
        get { return rigid.velocity; }
        private set { rigid.velocity = value; }
    }

    public Vector3 pos
    {
        get { return transform.position; }
        private set { transform.position = value; }
    }

    void LookAhead()
    {
        transform.LookAt(pos + rigid.velocity);
    }

    void Colorize()
    {
        Color randColor = Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1);
        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
        {
            r.material.color = randColor;
        }

        TrailRenderer trend = GetComponent<TrailRenderer>();
        trend.startColor = randColor;
        randColor.a = 0;
        trend.endColor = randColor;
        trend.endWidth = 0;
    }
}
