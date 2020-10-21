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
    [SerializeField] private float _camStartingDistanceBack = 12;
    [SerializeField] private float _midBodyLookHeight = 5;
    [SerializeField] private float _cameraManualTurnSpeed = 250f;
    [SerializeField] private float _camerAutoTurnSpeed = 155f;

    [SerializeField] private float _manualZoomingSpeed = 10;
    [SerializeField] private float _manualZoomOutDistance = 22;
    [SerializeField] private float _manualZoomInDistance = 6;
    [SerializeField] private float _manualZoomOutHeight = 15;
    [SerializeField] private float _manualZoomInHeight = 2;

    [SerializeField] private float _zoomBackSpeed = 20;

    [SerializeField] private float _autoZoomingSpeed = 2;
    [SerializeField] private float _autoZoomOutDistance = 14;
    [SerializeField] private float _autoZoomInDistance = 9;
    [SerializeField] private float _autoZoomOutHeight = 7;
    [SerializeField] private float _autoZoomInHeight = 5;

    [SerializeField] private Animator _strafeController;
    [SerializeField] private float _spinAroundTime = 0.19f;
    [SerializeField] private float _targetStrafeLookAroundTime = 0.6f;

    [SerializeField] private float _lookAtAngleChangeTime = .01f;
    [SerializeField] private float _lockOnTargetHeightTooHighAngleChange = 6.5f;

    [SerializeField] private Image _targetArrowImage;
    [SerializeField] private RectTransform _targetArrowTransform;
    [SerializeField] private Sprite _passiveTargetSprite;
    [SerializeField] private Sprite _mainTargetSprite;

    private float currentAngleDegrees;
    private Vector3 currentAngleVectorFromplayer;
    private float currentCamHeight;
    private Vector3 lockedTargetPositionOffset = new Vector3(0,0,0);
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

    
    private void FollowPlayerVertical()
    {
        if (lockedTarget.IsLockedOn) 
        {
            float heightDifference = ((lockedTarget.transform.position.y +lockedTarget.YOffset)- (player.transform.position.y + _midBodyLookHeight));
            float flatDifference =
                Mathf.Sqrt(lockedTarget.CurrentDistanceFromPlayer * lockedTarget.CurrentDistanceFromPlayer -
                           heightDifference * heightDifference);

            if (float.IsNaN(flatDifference))
                flatDifference = 0;

            float endDistance = flatDifference / 2 + _midBodyLookHeight * 2;
            float endHeight = (heightDifference > _lockOnTargetHeightTooHighAngleChange) ? 1.0f : (_midBodyLookHeight * 6 / 8 + (flatDifference / 3));
            
            currentCamDistanceBack += (endDistance - currentCamDistanceBack) * Time.deltaTime;
            currentCamHeight += (endHeight - currentCamHeight) * Time.deltaTime;

            /*currentCamDistanceBack = flatDifference/6 + _midBodyLookHeight*2;
            currentCamHeight = _midBodyLookHeight*6/8;*/
        }
        else if(Input.GetAxis(AxisInput.LEFT_VERTICAL) != 0 && !angleChanging && !strafeSpinCoroutine && !spinBackCoroutine)
        {
            if (!lockedTarget.IsLockedOn)
            {
                currentCamDistanceBack += Input.GetAxis(AxisInput.LEFT_VERTICAL) * Time.deltaTime * _autoZoomingSpeed *
                                          autoDistanceScaledIncrement;
                
                currentCamHeight -= Input.GetAxis(AxisInput.LEFT_VERTICAL) * Time.deltaTime * _autoZoomingSpeed * autoHeightScaledIncrement *
                                    (lockedTarget.IsLockedOn ? 2 : 1);
            }
        }

        if (!lockedTarget.IsLockedOn)
        {
            if (currentCamHeight < _autoZoomInHeight)
                currentCamHeight += Time.deltaTime * (_autoZoomInHeight - currentCamHeight) * _zoomBackSpeed;
            else if (currentCamHeight > _autoZoomOutHeight)
                currentCamHeight -= Time.deltaTime * (currentCamHeight - _autoZoomOutHeight) * _zoomBackSpeed;
            
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
        float minClamp = -1;
        float maxClamp = 1;

        float endHeight;

        if(!lockedTarget.IsLockedOn)
        {
            
            if (Input.GetAxis(AxisInput.RIGHT_VERTICAL) != 0 && !angleChanging && !strafeSpinCoroutine && !spinBackCoroutine)
            {
                if (currentCamDistanceBack <= _manualZoomInDistance)
                    maxClamp = 0;
                if (currentCamDistanceBack >= _manualZoomOutDistance)
                    minClamp = 0;
                //this clamp is so it can keep taking input from the joystick when it's reached one of it's two limits being 1 and -1
                currentCamDistanceBack -= Mathf.Clamp(Input.GetAxis(AxisInput.RIGHT_VERTICAL),minClamp,maxClamp) * Time.deltaTime * _manualZoomingSpeed *manualDistanceScaledIncrement;
            }
            
            if (currentCamDistanceBack < _manualZoomInDistance)
                currentCamDistanceBack +=
                    Time.deltaTime * (_manualZoomInDistance - currentCamDistanceBack) * _zoomBackSpeed;
            else if (currentCamDistanceBack > _manualZoomOutDistance)
                currentCamDistanceBack -=
                    Time.deltaTime * (currentCamDistanceBack - _manualZoomOutDistance) * _zoomBackSpeed;


            float camHeightDiff = _manualZoomOutHeight - _manualZoomInHeight;
            float camDistanceDiff = _manualZoomOutDistance - _manualZoomInDistance;

            endHeight = (currentCamDistanceBack - _manualZoomInDistance) / camDistanceDiff * camHeightDiff +
                              _manualZoomInHeight; // i just changed these two values from auto to manual. should probably make sure they're fine
            
        }
        else
        {
            
            float heightDifference = ((lockedTarget.transform.position.y +lockedTarget.YOffset)- (player.transform.position.y + _midBodyLookHeight));
            float flatDifference =
                Mathf.Sqrt(lockedTarget.CurrentDistanceFromPlayer * lockedTarget.CurrentDistanceFromPlayer -
                           heightDifference * heightDifference);

            if (float.IsNaN(flatDifference))
                flatDifference = 0;
            
            float minDistance = flatDifference / 3+ _midBodyLookHeight * 2;
            float maxDistance = flatDifference /2 + _midBodyLookHeight * 2;
            
            if (Input.GetAxis(AxisInput.RIGHT_VERTICAL) != 0 && !angleChanging && !strafeSpinCoroutine && !spinBackCoroutine)
            {
                if (currentCamDistanceBack <= minDistance)
                    maxClamp = 0;
                if (currentCamDistanceBack >= maxDistance)
                    minClamp = 0;
                //this clamp is so it can keep taking input from the joystick when it's reached one of it's two limits being 1 and -1
                currentCamDistanceBack -= Mathf.Clamp(Input.GetAxis(AxisInput.RIGHT_VERTICAL),minClamp,maxClamp) * Time.deltaTime * _manualZoomingSpeed;
            }
            
            if (currentCamDistanceBack < minDistance)
                currentCamDistanceBack +=
                    Time.deltaTime * (minDistance - currentCamDistanceBack) * _zoomBackSpeed;
            else if (currentCamDistanceBack > maxDistance)
                currentCamDistanceBack -=
                    Time.deltaTime * (currentCamDistanceBack - maxDistance) * _zoomBackSpeed;

            float startingAngle = currentAngleDegrees % 360;
            float endingAngle = ClosestAngleToBackOfPlayer();

            if (minDistance == maxDistance)
                endHeight = maxDistance;
            else
                endHeight = (((currentCamDistanceBack - minDistance) / (maxDistance - minDistance)) * //this line is the fraction of where the cam in respect to the min and max distance (ex: 4 is 3/4th in between min: 1 and max: 5)
                         ((heightDifference * 3 / 4) +_midBodyLookHeight - 1)) + 1;

            float angleDiff = Mathf.Abs(endingAngle - startingAngle);
            
            if (angleDiff > 60 && heightDifference > 0)
            {
                endHeight += heightDifference * 1.7f;
            }else if (angleDiff > 40 && heightDifference < 0)
            {
                endHeight += heightDifference * 2.2f;
            }

        }
        
        currentCamHeight += (endHeight - currentCamHeight) * Time.deltaTime;

        //currentCamHeight = Mathf.Clamp(currentCamHeight, 1, 25);
        
        

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

    private void LockedOnPerspective()
    {
        float currentAngle = currentAngleDegrees % 360;
        float endingAngle = ClosestAngleToBackOfPlayer();

        if (endingAngle - currentAngle >= 75) // this group of if statements gives the locked on camera boundaries (example, behind the back, should go to over the shoulder 20 degrees, behind the enemy sshould go to side view 75 degrees
        {
            endingAngle = endingAngle - 75;
        }else if (endingAngle - currentAngle > 20)
        {
            endingAngle = currentAngle;
        }else if (endingAngle - currentAngle >= 0)
        {
            endingAngle = endingAngle - 20;
        }else if (endingAngle - currentAngle <= -75)
        {
            endingAngle = endingAngle + 75;
        }else if (endingAngle - currentAngle < -20)
        {
            endingAngle = currentAngle;
        }
        else
        {
            endingAngle = endingAngle + 20;
        }
        currentAngleDegrees = currentAngle + (endingAngle - currentAngle) * Time.deltaTime;
        currentAngleVectorFromplayer = new Vector3(Mathf.Cos(currentAngleDegrees * Mathf.PI / 180), 0, Mathf.Sin(currentAngleDegrees * Mathf.PI / 180));
    }

    private float ClosestAngleToBackOfPlayer()
    {
        float startingAngle = currentAngleDegrees % 360;
        float endingAngle;
        
        endingAngle = Mathf.Atan2((-player.transform.forward).z,(-player.transform.forward).x) * 180 / Mathf.PI;
        
        if ((endingAngle-startingAngle) < -180) //we're adjusting the ending angle to compensate for the angle only being in the range 0-360
            endingAngle += 360;
        else if ((endingAngle - startingAngle > 180))
            endingAngle -= 360;

        return endingAngle;

    }

    private IEnumerator SpinToBack(bool auto, float distanceBack)
    {
        
        float startingAngle = currentAngleDegrees % 360;
        float endingAngle;
        float startingDistanceBack = currentCamDistanceBack;
        float startingCamHeight = currentCamHeight;
        float spinTime;

        /*Vector3 startingCameraLookOffset = lockedTargetPositionOffset;
        Vector3 endingCameraLookOffset;

        if (lockedTarget.IsLockedOn)
        {
            endingCameraLookOffset = new Vector3(,player.tr,);
        }
        else
        {
            endingCameraLookOffset = new Vector3(0,0,0);
        }*/

        
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

            endingAngle = ClosestAngleToBackOfPlayer();

            currentAngleDegrees = Mathf.Lerp(startingAngle, endingAngle, t / spinTime);
            currentAngleVectorFromplayer = new Vector3(Mathf.Cos(currentAngleDegrees * Mathf.PI / 180), 0, Mathf.Sin(currentAngleDegrees * Mathf.PI / 180));

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
    private void Update()
    {

        if(Input.GetAxis(AxisInput.LEFT_TRIGGER) != 0)
        {

            if (!strafing && !strafeSpinCoroutine && !spinBackCoroutine)
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
                if (Input.GetAxis(AxisInput.RIGHT_HORIZONTAL) != 0)
                    onFollow = false;
                
                if (lockedTarget.IsLockedOn && onFollow)
                {
                    LockedOnPerspective();
                }
                else
                {
                    ManualHorizontal();
                }
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
        
        if(lockedTarget.IsLockedOn && hasTarget && (!strafeSpinCoroutine && !spinBackCoroutine) && Input.GetAxis((AxisInput.LEFT_TRIGGER)) == 0) //when strafe button is let go, don't spin the camera back but leave target
        {
            hasTarget = false;
            lockedTarget.IsLockedOn = false;
        }
        else if (lockedTarget.CloseEnough == false && hasTarget && (!strafeSpinCoroutine && !spinBackCoroutine))
        {
            lockedTarget.IsLockedOn = false;
            
            if (interactables[0].CloseEnough) //switches target to new if you walk out of range and there is a target available
            {
                lockedTarget = interactables[0];
                lockedTarget.IsLockedOn = true;
                strafeSpinCoroutine = true;
                StartCoroutine(SpinToBack(true, currentCamDistanceBack));
            }
            else //else, spin to back but stay in strafe mode
            {
                hasTarget = false;
                spinBackCoroutine = true;
                StartCoroutine(SpinToBack(true, currentCamDistanceBack));
            }
        }
        
        if (Input.GetAxis(AxisInput.RIGHT_VERTICAL) != 0)
            onFollow = false;
        
        AdjustToLockedOnTarget();
        AdjustCamera(player.transform.position + new Vector3(0,_midBodyLookHeight,0));

        MoveAndArrangeTargetArrows();
    }

    private void AdjustToLockedOnTarget()
    {
        Vector3 endGoal;
        
        if (lockedTarget.IsLockedOn)
        {
            float heightDifference = ((lockedTarget.transform.position.y +lockedTarget.YOffset)- (player.transform.position.y + _midBodyLookHeight));
            float flatDifference =
                Mathf.Sqrt(lockedTarget.CurrentDistanceFromPlayer * lockedTarget.CurrentDistanceFromPlayer -
                           heightDifference * heightDifference);

            if (float.IsNaN(flatDifference))
                flatDifference = 0;
            
            endGoal = new Vector3(player.transform.forward.x * flatDifference/2, (heightDifference > _lockOnTargetHeightTooHighAngleChange)? heightDifference/4 : -_midBodyLookHeight/4,player.transform.forward.z * flatDifference/2);

            if(lockedTargetPositionOffset != endGoal)
            {
                lockedTargetPositionOffset += (endGoal - lockedTargetPositionOffset) * Time.deltaTime;
            }
        }
        else
        {
            endGoal = new Vector3(0,0,0);
            
            if(lockedTargetPositionOffset != endGoal)
            {
                lockedTargetPositionOffset += (endGoal - lockedTargetPositionOffset) * Time.deltaTime;
            }
        }
    }
    private void AdjustCamera(Vector3 whereToLook)
    {
        transform.position = player.transform.position + currentCamDistanceBack * currentAngleVectorFromplayer + new Vector3(0,currentCamHeight,0);
        
        //TODO: when terrain is added, make sure you are downwards casting to find out how high ground is and making sure you don't go through it

        transform.LookAt(whereToLook + new Vector3(lockedTargetPositionOffset.x, 
                                                                    lockedTargetPositionOffset.y, 
                                                                lockedTargetPositionOffset.z));
    }
    
    public GameObject Player => player;
    
    //QUICK SORT AND INTERACTABLE FUNCTIONS
    
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
}
