using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class DragAndThrow : MonoBehaviour
{
    private Camera cam;
    private Rigidbody rb; //Gameobject Must have a RigidBody Component.
    
    [Header("Trajectory Fields")] 
    [SerializeField] private float minForce;
    [SerializeField] private float maxForce;
    [SerializeField] private float power = 10;
    
    
    [Header("Line")] 
    [SerializeField] private LineRenderer lr;
    [SerializeField] private int lineSegments;
    [SerializeField] private float predictionTime;
    [SerializeField] private Gradient lineColor;
    [SerializeField] private Gradient lineDisableColor;
    
    //Force calculation fields
    private Vector3 screenPoint;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 force;
    
    void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        lineSegments = Mathf.FloorToInt(lineSegments * predictionTime); //Adaptive smoothing for line renderer
        lr.positionCount = lineSegments;
        lr.colorGradient = lineDisableColor;
    }
    void Update()
    {
        Drag();
    }
    void Drag()
    {
        if (Input.GetMouseButtonDown(0)) //Get start mouse point
        {
            lr.colorGradient = lineColor;
            screenPoint = cam.WorldToScreenPoint(transform.position);
            startPoint =  transform.position - cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,screenPoint.z));
        }
        if (Input.GetMouseButton(0)) //Calculate force due to mouse position and visualize trajectory with a line renderer
        {
            VisualizeTrajectory();
            screenPoint = cam.WorldToScreenPoint(transform.position);
            endPoint =  transform.position - cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,screenPoint.z));
            force = new Vector3(-1f*Mathf.Clamp(startPoint.x - endPoint.x,minForce,maxForce),3,-2f*Mathf.Clamp(startPoint.y - endPoint.y,minForce,maxForce));
        }
        if (Input.GetMouseButtonUp(0)) //Apply the calculated force when the mouse button is released
        {
            lr.colorGradient = lineDisableColor;
            rb.velocity = force * power;
        }
    }
    private void VisualizeTrajectory() //Draw the trajectory line
    {
        for (int i = 0; i < lineSegments; i++)
        {
            Vector3 pos = DrawTrajectory(force, (i * predictionTime) / (float) lineSegments); //Set line renderer positions for each time step
            lr.SetPosition(i,pos);
        }
    }
    Vector3 DrawTrajectory(Vector3 force, float time) //Calculate the necessary positions for line renderer
    {
        Vector3 vo = force * power;
        Vector3 Vxz = vo;
        Vxz.y = 0;

        Vector3 result = transform.position + vo * time;
        float sY = (-0.5f * Mathf.Abs(Physics.gravity.y) * (time * time)) + (vo.y * time) + transform.position.y;
        result.y = sY;
        
        return result;
    }
}
