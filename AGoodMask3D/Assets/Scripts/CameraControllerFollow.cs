using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private float _zoomBackSpeed = 10f;

    [SerializeField] private float _autoZoomingSpeed = 4;
    [SerializeField] private float _autoZoomOutDistance = 12;
    [SerializeField] private float _autoZoomInDistance = 8;
    [SerializeField] private float _autoZoomOutHeight = 8;
    [SerializeField] private float _autoZoomInHeight = 6;

    [SerializeField] private Animator _strafeController;
    [SerializeField] private float _spinAroundTime = 0.3f;
    [SerializeField] private float _targetStrafeLookAroundTime = 1.3f;

    [SerializeField] private float _lookAtAngleChangeTime = 1.5f;

    [SerializeField] private Image _targetArrowImage;
    [SerializeField] private RectTransform _targetArrowTransform;
    [SerializeField] private Sprite _passiveTargetSprite;
    [SerializeField] private Sprite _mainTargetSprite;

    private float currentAngleDegrees;
    private Vector3 currentAngleVectorFromplayer;
    private float currentCamHeight;
    private Vector3 currentLookAtPosition;
    private float manualDistanceScaledIncrement;
    private float autoDistanceScaledIncrement;
    private float manualHeightScaledIncrement;
    private float autoHeightScaledIncrement;

    private bool xPositive = false;
    private bool zPositive = false;
    private float currentCamDistanceBack;

    private bool onFollow = true;
    private bool strafeSpinCoroutine = false;
    private bool spinBackCoroutine = false;
    private bool strafing = false;

    private bool hasTarget = false;
    private bool facingPlayer = true;
    private bool angleChanging = false;
    
    
    private Interactable[] interactables = new Interactable[6] {new Interactable(), new Interactable(), new Interactable(), new Interactable(), new Interactable(), new Interactable()};
    private Interactable lockedTarget = new Interactable();

    public Interactable LockedTarget => lockedTarget;

    public bool StrafeSpinCoroutine => strafeSpinCoroutine;


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
        //playerRenderer = player.GetComponentInChildren<SkinnedMeshRenderer>();
        currentAngleVectorFromplayer = -player.transform.forward;
        currentAngleDegrees = 270;
        currentCamDistanceBack = _camStartingDistanceBack;
        currentLookAtPosition = player.transform.position + new Vector3(0, _midBodyLookHeight, 0);

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

    public void AddInteractableToList(Interactable obj)
    {
        interactables[interactables.Length - 1].AddedToList = false;
        interactables[interactables.Length - 1] = obj;
    }

    private void QuickSortInteractables(int low, int high) //quicksort close interactable objects for use of targetting arrow and lock on targetting
    {
        if (low < high)
        {
            int i = PartitionInteractables(low, high);
            
            QuickSortInteractables(low,i-1);
            QuickSortInteractables(i+1,high);
        }
    }

    private int PartitionInteractables(int low, int high) //partitioning used for the quick sort function
    {
        Interactable pivot = interactables[high];
        int i = low - 1;

        for (int j = low; j < high; j++)
        {

            if (!interactables[j].CloseEnough)
            {
                interactables[j].AddedToList = false;
                interactables[j] = new Interactable();
            }
            
            if (interactables[j].CurrentDistanceFromPlayer <= pivot.CurrentDistanceFromPlayer)
            {
                i++;

                Interactable temp = interactables[i];
                interactables[i] = interactables[j];
                interactables[j] = temp;
            }
            
        }
        
        Interactable temp2 = interactables[i + 1];
        interactables[i + 1] = interactables[high];
        interactables[high] = temp2;

        return i + 1;

    }
    private void FollowPlayerVertical()
    {
        if(Input.GetAxis(AxisInput.LEFT_VERTICAL) != 0 && !angleChanging && !strafeSpinCoroutine && !spinBackCoroutine)
        {
            if (!lockedTarget.IsLockedOn)
            {
                currentCamDistanceBack += Input.GetAxis(AxisInput.LEFT_VERTICAL) * Time.deltaTime * _autoZoomingSpeed *
                                          autoDistanceScaledIncrement *
                                          (lockedTarget.IsLockedOn
                                              ? 2
                                              : 1
                                          ); //make this be the distance from the player instead of move with the joystick
            }

            currentCamHeight -= Input.GetAxis(AxisInput.LEFT_VERTICAL) * Time.deltaTime * _autoZoomingSpeed * autoHeightScaledIncrement *
                                (lockedTarget.IsLockedOn ? 2 : 1);
        }
        
        if (currentCamHeight < _autoZoomInHeight)
            currentCamHeight += Time.deltaTime * (_autoZoomInHeight - currentCamHeight) *_zoomBackSpeed;
        else if (currentCamHeight > _autoZoomOutHeight)
            currentCamHeight -= Time.deltaTime * (currentCamHeight - _autoZoomOutHeight) *_zoomBackSpeed;
            
        if (lockedTarget.IsLockedOn) //if strafing with target
        {
            if (currentCamDistanceBack < _autoZoomInDistance)
            {
                currentCamDistanceBack +=
                    Time.deltaTime * (_autoZoomInDistance - currentCamDistanceBack) *_zoomBackSpeed/5;
            }else if (currentCamDistanceBack > lockedTarget.CurrentDistanceFromPlayer)
            {
                currentCamDistanceBack -=
                    Time.deltaTime * (currentCamDistanceBack - lockedTarget.CurrentDistanceFromPlayer) *_zoomBackSpeed/5;
            }
        }
        else
        {
            if (currentCamDistanceBack < _autoZoomInDistance)
            {
                currentCamDistanceBack +=
                    Time.deltaTime * (_autoZoomInDistance - currentCamDistanceBack) *_zoomBackSpeed;
            }else if (currentCamDistanceBack > _autoZoomOutDistance)
            {
                currentCamDistanceBack -=
                    Time.deltaTime * (currentCamDistanceBack - _autoZoomOutDistance) *_zoomBackSpeed;
            }
        }
    }

    private void FollowPlayerHorizontal()
    {
        if (Input.GetAxis(AxisInput.LEFT_HORIZONTAL) != 0 && !angleChanging && !strafeSpinCoroutine && !spinBackCoroutine)
        {
            currentAngleDegrees -= Input.GetAxis(AxisInput.LEFT_HORIZONTAL) * Time.deltaTime * _camerAutoTurnSpeed;
            currentAngleVectorFromplayer = new Vector3(Mathf.Cos(currentAngleDegrees * Mathf.PI / 180), 0, Mathf.Sin(currentAngleDegrees * Mathf.PI / 180));
        }
    }
    
    private void ManualHorizontal()
    {
        if (Input.GetAxis(AxisInput.RIGHT_HORIZONTAL) != 0 && !angleChanging && !strafeSpinCoroutine && !spinBackCoroutine)
        {
            currentAngleDegrees -= Input.GetAxis(AxisInput.RIGHT_HORIZONTAL) * Time.deltaTime * _cameraManualTurnSpeed;
            currentAngleVectorFromplayer = new Vector3(Mathf.Cos(currentAngleDegrees * Mathf.PI / 180), 0, Mathf.Sin(currentAngleDegrees * Mathf.PI / 180));
        }
    }

    private void ManualVertical()
    {
        if (Input.GetAxis(AxisInput.RIGHT_VERTICAL) != 0 && !angleChanging && !strafeSpinCoroutine && !spinBackCoroutine)
        {
            float minClamp = -1;
            float maxClamp = 1;
            if (currentCamDistanceBack <= _manualZoomInDistance)
                maxClamp = 0;
            if (currentCamDistanceBack >= _manualZoomOutDistance)
                minClamp = 0;
            
            currentCamDistanceBack -= Mathf.Clamp(Input.GetAxis(AxisInput.RIGHT_VERTICAL),minClamp,maxClamp) * Time.deltaTime * _manualZoomingSpeed *manualDistanceScaledIncrement;
            /*if(currentCamHeight <= _manualZoomOutHeight && currentCamHeight >= _manualZoomInHeight)
                currentCamHeight -= Input.GetAxis(AxisInput.RIGHT_VERTICAL) * Time.deltaTime * _manualZoomingSpeed * manualHeightScaledIncrement;*/
        }
        
        /*if (currentCamHeight < _manualZoomInHeight)
            currentCamHeight += Time.deltaTime * (_manualZoomInHeight - currentCamHeight) *_zoomBackSpeed;
        else if (currentCamHeight > _manualZoomOutHeight)
            currentCamHeight -= Time.deltaTime * (currentCamHeight - _manualZoomOutHeight) *_zoomBackSpeed;*/
            
        if (currentCamDistanceBack < _manualZoomInDistance)
            currentCamDistanceBack += Time.deltaTime * (_manualZoomInDistance - currentCamDistanceBack) *_zoomBackSpeed;
        else if (currentCamDistanceBack > _manualZoomOutDistance)
            currentCamDistanceBack -= Time.deltaTime * (currentCamDistanceBack - _manualZoomOutDistance) *_zoomBackSpeed;
        
        
        float camHeightDiff = _manualZoomOutHeight - _manualZoomInHeight;
        float camDistanceDiff = _manualZoomOutDistance - _manualZoomInDistance;
        currentCamHeight = (currentCamDistanceBack - _autoZoomInDistance) / camDistanceDiff * camHeightDiff + _autoZoomInHeight;
    }
    
    private void Update()
    {

        if(Input.GetAxis(AxisInput.LEFT_TRIGGER) != 0)
        {

            if (!strafing)
            {
                if (interactables[0].CloseEnough)
                {
                    lockedTarget = interactables[0];
                    lockedTarget.IsLockedOn = true;
                    hasTarget = true;
                }
                strafeSpinCoroutine = true;
                StartCoroutine(SpinToBack(true,_camStartingDistanceBack));
                onFollow = true;
            }
            
            if(!strafeSpinCoroutine)
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

            strafing = false;
            if(onFollow)
            {
                FollowPlayer();
            }else
            {
                ManualPlacement();
            }
            
            if (Input.GetAxis(AxisInput.RIGHT_HORIZONTAL) != 0)
                onFollow = false;
        }
        
        _strafeController.SetBool("StrifeOn",(strafing || strafeSpinCoroutine));
        
        if (lockedTarget.IsLockedOn == false && hasTarget && (!strafeSpinCoroutine || !spinBackCoroutine))
        {
            hasTarget = false;
            spinBackCoroutine = true;
            StartCoroutine(SpinToBack(true, currentCamDistanceBack));
        }
        
        if (Input.GetAxis(AxisInput.RIGHT_VERTICAL) != 0)
            onFollow = false;

        /*if(lockedTarget.IsLockedOn)
        {
            if (facingPlayer)
            {
                angleChanging = true;
                StartCoroutine(ChangeCameraLookAtAngle(player.transform.position + new Vector3(0, _midBodyLookHeight, 0)
                    , (player.transform.position + new Vector3(0, _midBodyLookHeight, 0) +
                       lockedTarget.transform.position + new Vector3(0, lockedTarget.YOffset, 0)) / 2));
                facingPlayer = false;
            }
            else if (!angleChanging)
            {
                AdjustCamera((player.transform.position + new Vector3(0, _midBodyLookHeight, 0) +
                              lockedTarget.transform.position + new Vector3(0, lockedTarget.YOffset, 0)) /
                             2); //average between player position and locked on target}
            }
        }else
        {
            if(!facingPlayer)
            {

                angleChanging = true;
                StartCoroutine(ChangeCameraLookAtAngle((player.transform.position + new Vector3(0, _midBodyLookHeight, 0) +
                                                        lockedTarget.transform.position + new Vector3(0, lockedTarget.YOffset, 0)) / 2,player.transform.position + new Vector3(0, _midBodyLookHeight, 0)));
                facingPlayer = true;
            }
            else if(!angleChanging)
            {*/
                AdjustCamera(player.transform.position + new Vector3(0,_midBodyLookHeight,0));
            /*}
            
        }*/

        MoveAndArrangeTargetArrows();
    }

    IEnumerator ChangeCameraLookAtAngle(Vector3 starting, Vector3 ending)
    {
        for (float t = 0; t <= _lookAtAngleChangeTime; t += Time.deltaTime)
        {
            Vector3 newAngleVector = starting + (ending - starting) * t / _lookAtAngleChangeTime;
            AdjustCamera(newAngleVector);
            yield return null;
        }
        angleChanging = false;
    }

    private void MoveAndArrangeTargetArrows()
    {
        QuickSortInteractables(0,interactables.Length-1);

        if (lockedTarget.IsLockedOn)
        {
            _targetArrowImage.sprite = _mainTargetSprite;
            _targetArrowTransform.gameObject.SetActive(true);
            _targetArrowTransform.position = Camera.main.WorldToScreenPoint(lockedTarget.transform.position+new Vector3(0,lockedTarget.YOffset,0));
        }
        else if (interactables[0].CloseEnough)
        {
            _targetArrowImage.sprite = _passiveTargetSprite;
            _targetArrowTransform.gameObject.SetActive(true);
            _targetArrowTransform.position = Camera.main.WorldToScreenPoint(interactables[0].transform.position+new Vector3(0,interactables[0].YOffset,0));
        }
        else
        {
            _targetArrowTransform.gameObject.SetActive(false);
        }
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

    private IEnumerator SpinToBack(bool auto, float distanceBack)
    {
        
        float startingAngle = currentAngleDegrees % 360;
        float endingAngle;
        float startingDistanceBack = currentCamDistanceBack;
        float startingCamHeight = currentCamHeight;
        float spinTime;
            
        endingAngle = Mathf.Atan2((-player.transform.forward).z,(-player.transform.forward).x) * 180 / Mathf.PI;
        
        if(lockedTarget.IsLockedOn)
        {
            spinTime = _targetStrafeLookAroundTime;
        }
        else
        {
            spinTime = _spinAroundTime;
        }
        for(float t = 0; t< spinTime; t += Time.deltaTime)
        {
            
            if ((endingAngle-startingAngle) < -180) //we're adjusting the ending angle to compensate for the angle only being in the range 0-360
                endingAngle += 360;
            else if ((endingAngle - startingAngle > 180))
                endingAngle -= 360;
            
            Debug.Log($"new ending angle:{endingAngle}");
            
            currentAngleDegrees = Mathf.Lerp(startingAngle, endingAngle, t / spinTime);
            currentAngleVectorFromplayer = new Vector3(Mathf.Cos(currentAngleDegrees * Mathf.PI / 180), 0, Mathf.Sin(currentAngleDegrees * Mathf.PI / 180)); // I think I gotta mess with stuff in here

            currentCamDistanceBack = Mathf.Lerp(startingDistanceBack, distanceBack, t / spinTime);
            
            if(auto)
            {
                currentCamHeight = Mathf.Lerp(currentCamHeight,
                    
                    (_camStartingDistanceBack - _autoZoomInDistance) / (_autoZoomOutDistance - _autoZoomInDistance) *
                    (_autoZoomOutHeight - _autoZoomInHeight) +
                    _autoZoomInHeight, //this mess of a function, this second variable is the same at the end of DetermineCameraDistanceHeightVariables() it's just got it's variables not condensed and it's the manual form
                    
                    t / spinTime);
            }
            else
            {
                currentCamHeight = Mathf.Lerp(currentCamHeight,
                    
                    (_camStartingDistanceBack - _manualZoomInDistance) / (_manualZoomOutDistance - _manualZoomInDistance) *
                    (_manualZoomOutHeight - _manualZoomInHeight) +
                    _manualZoomInHeight, //this mess of a function, this second variable is the same at the end of DetermineCameraDistanceHeightVariables() it's just got it's variables not condensed and it's the manual form
                    
                    t / spinTime);
            }
            
            yield return null;
        }

        strafeSpinCoroutine = false;
        spinBackCoroutine = false;
    }
    
    // Update is called once per frame
    private void AdjustCamera(Vector3 whereToLook)
    {
        transform.position = player.transform.position + currentCamDistanceBack * currentAngleVectorFromplayer + new Vector3(0,currentCamHeight,0);

        /*if(spinCoroutine)
        {
            currentLookAtPosition -= (currentLookAtPosition - whereToLook) * Time.deltaTime;
            transform.LookAt(currentLookAtPosition);
        }
        else
        {*/
        transform.LookAt(whereToLook);
        //}
    }

    private void AdjustTransparency()
    {
        if (currentCamDistanceBack < 5)
        {
            float camRatio = (currentCamDistanceBack - 1) / 4;
            
            if (camRatio < 0)
                camRatio = 0;
             //playerRenderer.material.SetTexture("_Texture",); 
            //playerRenderer.material.SetVector("_Alpha",new Vector4(camRatio,0,0,0)); 
        }
        else
        {
            //playerRenderer.material.SetVector("_Alpha",new Vector4(1,0,0,0));  
        }
    }

    public GameObject Player => player;
}
