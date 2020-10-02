using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
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

    [SerializeField] private Animator _strafeController;
    [SerializeField] private float _strafeLookAroundTime = 1;

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
    private bool strafeCoroutine = false;
    private bool strafing = false;

    private Renderer playerRenderer;
    

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
        playerRenderer = player.GetComponentInChildren<Renderer>();
        currentAngleVectorFromplayer = -player.transform.forward;
        currentAngleDegrees = 270;
        currentCamDistanceBack = _camStartingDistanceBack;

        DetermineCameraDistanceHeightVariables();
    }

    private void FollowPlayer()
    {
        FollowPlayerHorizontal();
        FollowPlayerVertical();
    }

    private void ManualPlacement()
    {
        ManualHorizontal();
        ManualVertical();
    }

    private void FollowPlayerVertical()
    {
        if(Input.GetAxis(AxisInput.LEFT_VERTICAL) != 0)
        {
            currentCamDistanceBack += Input.GetAxis(AxisInput.LEFT_VERTICAL) * Time.deltaTime * _autoZoomingSpeed *autoDistanceScaledIncrement;
            currentCamHeight -= Input.GetAxis(AxisInput.LEFT_VERTICAL) * Time.deltaTime * _autoZoomingSpeed * autoHeightScaledIncrement;
            
            currentCamHeight = Mathf.Clamp(currentCamHeight, _autoZoomInHeight, _autoZoomOutHeight);
            currentCamDistanceBack = Mathf.Clamp(currentCamDistanceBack,_autoZoomInDistance, _autoZoomOutDistance);
        }
    }

    private void FollowPlayerHorizontal()
    {
        if (Input.GetAxis(AxisInput.LEFT_HORIZONTAL) != 0)
        {
            currentAngleDegrees -= Input.GetAxis(AxisInput.LEFT_HORIZONTAL) * Time.deltaTime * _camerAutoTurnSpeed;
            currentAngleVectorFromplayer = new Vector3(Mathf.Cos(currentAngleDegrees * Mathf.PI / 180), 0, Mathf.Sin(currentAngleDegrees * Mathf.PI / 180));
        }
    }

    private void ManualHorizontal()
    {
        if (Input.GetAxis(AxisInput.RIGHT_HORIZONTAL) != 0)
        {
            currentAngleDegrees -= Input.GetAxis(AxisInput.RIGHT_HORIZONTAL) * Time.deltaTime * _cameraManualTurnSpeed;
            currentAngleVectorFromplayer = new Vector3(Mathf.Cos(currentAngleDegrees * Mathf.PI / 180), 0, Mathf.Sin(currentAngleDegrees * Mathf.PI / 180));
        }
    }

    private void ManualVertical()
    {
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

        if(Input.GetAxis(AxisInput.LEFT_TRIGGER) != 0)
        {
            if (!strafing)
            {
                strafeCoroutine = true;
                StartCoroutine(StartStrafe());
                onFollow = true;
            }
            
            if(!strafeCoroutine)
            {
                ManualHorizontal();
                if(onFollow)
                    FollowPlayerVertical();
                else
                    ManualVertical();
            }
            
            strafing = true;
        }
        else
        {
            if(onFollow)
            {
                strafing = false;
                FollowPlayer();
            }else
            {
                strafing = false;
                ManualPlacement();
            }
            
            if (Input.GetAxis(AxisInput.RIGHT_HORIZONTAL) != 0)
                onFollow = false;
        }
        
        if (Input.GetAxis(AxisInput.RIGHT_VERTICAL) != 0)
            onFollow = false;
        
        _strafeController.SetBool("StrifeOn",strafing);

        AdjustTransparency();
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
        
        currentCamHeight = (currentCamDistanceBack - _autoZoomInDistance) / camDistanceDiff * camHeightDiff + _autoZoomInHeight; // adjust the height to the starting distance
    }
    
    private IEnumerator StartStrafe()
    {
        
        float startingAngle = currentAngleDegrees % 360;
        float endingAngle;
        float startingDistanceBack = currentCamDistanceBack;
        float startingCamHeight = currentCamHeight;

        

        for(float t = 0; t< _strafeLookAroundTime; t += Time.deltaTime)
        {
            endingAngle = Mathf.Atan2((-player.transform.forward).z,(-player.transform.forward).x) * 180 / Mathf.PI;
            if (startingAngle >= 180)
                endingAngle += 360;
            
            currentAngleDegrees = Mathf.Lerp(startingAngle, endingAngle, t / _strafeLookAroundTime);
            currentAngleVectorFromplayer = new Vector3(Mathf.Cos(currentAngleDegrees * Mathf.PI / 180), 0, Mathf.Sin(currentAngleDegrees * Mathf.PI / 180));

            currentCamDistanceBack = Mathf.Lerp(startingDistanceBack, _camStartingDistanceBack, t / _strafeLookAroundTime);
            currentCamHeight = Mathf.Lerp(currentCamHeight,
                                        (_camStartingDistanceBack - _manualZoomInDistance) / (_manualZoomOutDistance - _manualZoomInDistance) * 
                                            (_manualZoomOutHeight - _manualZoomInHeight) + _manualZoomInHeight, //this mess of a function, this second variable is the same at the end of DetermineCameraDistanceHeightVariables() it's just got it's variables not condensed and it's the manual form
                                        t / _strafeLookAroundTime); 
            
            yield return null;
        }

        strafeCoroutine = false;
    }
    
    // Update is called once per frame
    private void AdjustCamera()
    {
        transform.position = player.transform.position + currentCamDistanceBack * currentAngleVectorFromplayer + new Vector3(0,currentCamHeight,0);
        transform.LookAt(player.transform.position + new Vector3(0,_midBodyLookHeight,0));

    }

    private void AdjustTransparency()
    {
        /*if (currentCamDistanceBack < 5)
        {
            float camRatio = (currentCamDistanceBack - 1) / 4;
            Mathf.Clamp(camRatio, 0, 1);
             //playerRenderer.material.SetTexture("_Texture",); 
            playerRenderer.material.SetFloat("_Alpha",camRatio); 
        }
        else
        {
            playerRenderer.material.SetFloat("_Alpha",1); 
        }*/
    }
}
