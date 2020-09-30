using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerFollow : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float _camStartingDistanceBack = 9;
    [SerializeField] private float _midBodyLookHeight = 5;
    [SerializeField] private float _cameraManualTurnSpeed = 250f;
    [SerializeField] private float _camerAutoTurnSpeed = 155f;

    [SerializeField] private float _manualZoomingSpeed = 10;
    [SerializeField] private float _manualZoomOutDistance = 22;
    [SerializeField] private float _manualZoomInDistance = 6;
    [SerializeField] private float _manualZoomOutHeight = 15;
    [SerializeField] private float _manualZoomInHeight = 2;

    [SerializeField] private float _autoZoomingSpeed = 4;
    [SerializeField] private float _autoZoomOutDistance = 12;
    [SerializeField] private float _autoZoomInDistance = 8;
    [SerializeField] private float _autoZoomOutHeight = 8;
    [SerializeField] private float _autoZoomInHeight = 6;

    private float currentAngleDegrees;
    private Vector3 currentAngleVectorFromplayer;
    private float currentCamHeight;
    private float manualDistanceScaledIncrement;
    private float autoDistanceScaledIncrement;
    private float manualHeightScaledIncrement;
    private float autoHeightScaledIncrement;

    private bool xPositive = false;
    private bool zPositive = false;
    private float currentCamDistanceBack;

    private bool onFollow = true;

    public static class AxisInput {
        public const string LEFT_HORIZONTAL = "Horizontal";
        public const string LEFT_VERTICAL = "Vertical";
        public const string RIGHT_HORIZONTAL = "RHorizontal";
        public const string RIGHT_VERTICAL = "RVertical";
        public const string LEFT_TRIGGER = "LTrigger";
        public const string RIGHT_TRIGGER = "RTrigger";
    }


    void Start()
    {
        currentAngleVectorFromplayer = -player.transform.forward;
        currentAngleDegrees = 270;
        currentCamDistanceBack = _camStartingDistanceBack;

        DetermineCameraDistanceHeightVariables();
    }

    private void StrafeCamera()
    {
        if (Input.GetAxis(AxisInput.RIGHT_HORIZONTAL) != 0)
        {
            currentAngleDegrees -= Input.GetAxis(AxisInput.RIGHT_HORIZONTAL) * Time.deltaTime * _cameraManualTurnSpeed;
            currentAngleVectorFromplayer = new Vector3(Mathf.Cos(currentAngleDegrees * Mathf.PI / 180), 0, Mathf.Sin(currentAngleDegrees * Mathf.PI / 180));
        }
        
        if(Input.GetAxis(AxisInput.LEFT_VERTICAL) != 0)
        {
            currentCamDistanceBack += Input.GetAxis(AxisInput.LEFT_VERTICAL) * Time.deltaTime * _autoZoomingSpeed *autoDistanceScaledIncrement; //working on this stuff
            currentCamHeight -= Input.GetAxis(AxisInput.LEFT_VERTICAL) * Time.deltaTime * _autoZoomingSpeed * autoHeightScaledIncrement;
            
            currentCamHeight = Mathf.Clamp(currentCamHeight, _autoZoomInHeight, _autoZoomOutHeight);
            currentCamDistanceBack = Mathf.Clamp(currentCamDistanceBack,_autoZoomInDistance, _autoZoomOutDistance);
        }
    }

    private void FollowPlayer()
    {
        if (Input.GetAxis(AxisInput.LEFT_HORIZONTAL) != 0)
        {
            currentAngleDegrees -= Input.GetAxis(AxisInput.LEFT_HORIZONTAL) * Time.deltaTime * _camerAutoTurnSpeed;
            currentAngleVectorFromplayer = new Vector3(Mathf.Cos(currentAngleDegrees * Mathf.PI / 180), 0, Mathf.Sin(currentAngleDegrees * Mathf.PI / 180));
        }
        
        if(Input.GetAxis(AxisInput.LEFT_VERTICAL) != 0)
        {
            currentCamDistanceBack += Input.GetAxis(AxisInput.LEFT_VERTICAL) * Time.deltaTime * _autoZoomingSpeed *autoDistanceScaledIncrement; //working on this stuff
            currentCamHeight -= Input.GetAxis(AxisInput.LEFT_VERTICAL) * Time.deltaTime * _autoZoomingSpeed * autoHeightScaledIncrement;
            
            currentCamHeight = Mathf.Clamp(currentCamHeight, _autoZoomInHeight, _autoZoomOutHeight);
            currentCamDistanceBack = Mathf.Clamp(currentCamDistanceBack,_autoZoomInDistance, _autoZoomOutDistance);
        }
            
    }

    private void ManualPlacement()
    {
        if (Input.GetAxis(AxisInput.RIGHT_HORIZONTAL) != 0)
        {
            currentAngleDegrees -= Input.GetAxis(AxisInput.RIGHT_HORIZONTAL) * Time.deltaTime * _cameraManualTurnSpeed;
            currentAngleVectorFromplayer = new Vector3(Mathf.Cos(currentAngleDegrees * Mathf.PI / 180), 0, Mathf.Sin(currentAngleDegrees * Mathf.PI / 180));
        }

        if (Input.GetAxis(AxisInput.RIGHT_VERTICAL) != 0)
        {
            currentCamDistanceBack -= Input.GetAxis(AxisInput.RIGHT_VERTICAL) * Time.deltaTime * _manualZoomingSpeed *manualDistanceScaledIncrement;
            currentCamHeight -= Input.GetAxis(AxisInput.RIGHT_VERTICAL) * Time.deltaTime * _manualZoomingSpeed * manualHeightScaledIncrement;
            
            currentCamHeight = Mathf.Clamp(currentCamHeight, _manualZoomInHeight, _manualZoomOutHeight);
            currentCamDistanceBack = Mathf.Clamp(currentCamDistanceBack,_manualZoomInDistance, _manualZoomOutDistance);
        }
    }
    
    private void Update()
    {
        if (Input.GetAxis(AxisInput.RIGHT_HORIZONTAL) != 0 || Input.GetAxis(AxisInput.RIGHT_VERTICAL) != 0)
            onFollow = false;

        if(Input.GetAxis(AxisInput.LEFT_TRIGGER) != 0)
        {
            onFollow = true;
            StrafeCamera();
        }
        else if(onFollow)
            FollowPlayer();
        else
            ManualPlacement();
        
        AdjustCamera();
    }

    private void DetermineCameraDistanceHeightVariables()
    {
        float camHeightDiff = _manualZoomOutHeight - _manualZoomInHeight;
        float camDistanceDiff = _manualZoomOutDistance - _manualZoomInDistance;

        if (camHeightDiff > camDistanceDiff) //find out the difference between max/min zoom distances and heights and scale one or the other accordingly so they keep in sync
        {
            manualHeightScaledIncrement = camHeightDiff / camDistanceDiff;
            manualDistanceScaledIncrement = 1;
        }
        else
        {
            manualDistanceScaledIncrement = camDistanceDiff / camHeightDiff;
            manualHeightScaledIncrement = 1;
        }

        camHeightDiff = _autoZoomOutHeight - _autoZoomInHeight;
        camDistanceDiff = _autoZoomOutDistance - _autoZoomInDistance;
        
        if (camHeightDiff > camDistanceDiff) //find out the difference between max/min zoom distances and heights and scale one or the other accordingly so they keep in sync
        {
            autoHeightScaledIncrement = camHeightDiff / camDistanceDiff;
            autoDistanceScaledIncrement = 1;
        }
        else
        {
            autoDistanceScaledIncrement = camDistanceDiff / camHeightDiff;
            autoHeightScaledIncrement = 1;
        }
        
        currentCamHeight = currentCamDistanceBack / camDistanceDiff * camHeightDiff + _autoZoomInHeight; // adjust the height to the starting distance
    }
    
    // Update is called once per frame
    private void AdjustCamera()
    {
        
        transform.position = player.transform.position + currentCamDistanceBack * currentAngleVectorFromplayer + new Vector3(0,currentCamHeight,0);
        transform.LookAt(player.transform.position + new Vector3(0,_midBodyLookHeight,0));
        /*var posZ = new Vector3(transform.forward.x * -5.5f,2f,transform.forward.z * -5.5f); // THIS -5 NEEDS TO BE ADJUSTABLE WITH R STICK
        transform.position = transform.position + (posZ);

        var xDiff = player.transform.position.x - transform.position.x;
        var zDiff = player.transform.position.z - transform.position.z;

        var newAngle = Mathf.Atan(xDiff / zDiff) * 180/Mathf.PI;

        if (zDiff <= 0)
            newAngle = newAngle + 180;

        var currentAngle = transform.eulerAngles.y;
        var angleDiff = newAngle - currentAngle;

        if (angleDiff > 200)
            angleDiff -= 360;
        else if (angleDiff < -200)
            angleDiff += 360;*/

        /*if (currentAngle == newAngle)
            lastSpot = transform.position;

        Debug.LogError(lastSpot);
        */

        /*if(Vector3.Distance(lastSpot, transform.position) > 2) //2 is the distance you need to move beforte the camera starts following you
            transform.eulerAngles = new Vector3(0, 
                ((angleDiff/20)>newAngle) ? (newAngle) : (currentAngle +angleDiff / 20), 0); //20 is the number for how slow it pans back to the player
                */



    }
}
