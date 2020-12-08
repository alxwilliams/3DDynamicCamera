using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.XR;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private Camera cam;
    [SerializeField] private float _camDefaultHeight = -2.7f;
    [SerializeField] private float speed = 20;
    [SerializeField] private float strafeSpeed = 10;
    [SerializeField] private float _fallSpeed = 10f;
    private bool _usingKeyboard = false;
    private bool _rightArrow = false;
    private bool _leftArrow = false;
    private bool _upArrow = false;
    private bool _downArrow = false;
    private bool _walking = false;
    private float _zDirection = 0;
    private float _xDirection = 0;

    private Vector3 lastSpot;
    [SerializeField] private CameraControllerFollow camScript;
    private Rigidbody rb;
    [SerializeField] private BoxCollider bc;

    private float groundDistance = 1000;
    
    private bool isGrounded = false;
    private Vector3 currentGravity;
    ContactPoint[] cPoints; 
    public float maxGroundedAngle = 45f;
    Vector3 groundNormal;
    private float objectSlantAngle = 0;
    private float angleFromObject = 0;
    
    
    public static class AxisInput {
        public const string LEFT_HORIZONTAL = "Horizontal";
        public const string LEFT_VERTICAL = "Vertical";
        public const string RIGHT_HORIZONTAL = "RHorizontal";
        public const string RIGHT_VERTICAL = "RVertical";
        public const string LEFT_TRIGGER = "LTrigger";
    }
    
    private void FixedUpdate()
    {
        _upArrow = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W); //TODO: make these keys changeable
        _downArrow = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
        _leftArrow = Input.GetKey((KeyCode.LeftArrow)) || Input.GetKey(KeyCode.A);
        _rightArrow = Input.GetKey((KeyCode.RightArrow)) || Input.GetKey(KeyCode.D);
        
        if (_upArrow || _downArrow || _leftArrow || _rightArrow) //determines whether to use controller or keyboard input
            _usingKeyboard = true;
        else if (Input.GetAxis(AxisInput.LEFT_HORIZONTAL) != 0 || Input.GetAxis(AxisInput.LEFT_VERTICAL) != 0)
            _usingKeyboard = false;
        
        if(_usingKeyboard)
            KeyboardMovement();
        else
        {
            _xDirection = Input.GetAxis(AxisInput.LEFT_HORIZONTAL);
            _zDirection = Input.GetAxis(AxisInput.LEFT_VERTICAL);
        }
        Movement();
    }
    
    private void KeyboardMovement()
    {
        if(_upArrow && _downArrow){
            _zDirection = 0;
        }else if (_upArrow){
            _zDirection =1;
        }else if (_downArrow){
            _zDirection = -1;
        }else{
            _zDirection = 0;
        }

        if(_leftArrow && _rightArrow){
            _xDirection = 0;
        }else if (_rightArrow){
            _xDirection =1;
        }else if (_leftArrow){
            _xDirection = -1;
        }else{
            _xDirection = 0;
        }
        
        if (_xDirection != 0 && _zDirection != 0)
        {
            _xDirection = _xDirection * Mathf.Acos(45 * Mathf.PI/180);
            _zDirection = _zDirection * Mathf.Acos(45 * Mathf.PI/180);
        }
    }

    private void Movement(){

        //RayCastCollision(); //keep raycast collision here and stop x or z direction so it sets the animation float correctly

        
        if(Mathf.Abs(_zDirection) > Mathf.Abs(_xDirection))
            anim.SetFloat("Speed",Mathf.Abs(_zDirection));
        else
            anim.SetFloat("Speed",Mathf.Abs(_xDirection));

        Vector3 moveX;
        Vector3 moveZ;
        
        
        if(camScript.LockedTarget != null &&
           (camScript.LockedTarget.IsLockedOn || Input.GetAxis(AxisInput.LEFT_TRIGGER) != 0))
        {
            anim.SetBool("Strafing", true);
            moveX = _xDirection * strafeSpeed * new Vector3(cam.transform.right.x,0,cam.transform.right.z);
            moveZ = _zDirection * strafeSpeed * new Vector3(cam.transform.forward.x,0,cam.transform.forward.z);
        }
        else
        {
            anim.SetBool("Strafing", false);
            moveX = _xDirection * speed * new Vector3(cam.transform.right.x,0,cam.transform.right.z);
            moveZ = _zDirection * speed * new Vector3(cam.transform.forward.x,0,cam.transform.forward.z);
        }

        var movement = moveX + moveZ;
        
        /*if (rb.velocity.x == 0)
            movement = new Vector3(0,0,movement.z);

        if (rb.velocity.z == 0)
            movement = new Vector3(movement.x,0,0);
            */
        
        
        
        anim.SetFloat("ForwardMotion", _zDirection);
        anim.SetFloat("HorizontalMotion",_xDirection);
            
        movement *= Time.deltaTime;
        rb.velocity = new Vector3(0,rb.velocity.y,0);

        
        if(_xDirection != 0 || _zDirection !=0)
        {
            if (Input.GetAxis(AxisInput.LEFT_TRIGGER) == 0)
            {
                //make player face target if player here instead of no turning
                transform.rotation = Quaternion.LookRotation(movement); //will have to start strafe animation here
            }
        }
        
        if (camScript.LockedTarget != null && camScript.LockedTarget.IsLockedOn)
        {
            transform.LookAt( new Vector3(camScript.LockedTarget.transform.position.x,transform.position.y,camScript.LockedTarget.transform.position.z));
        }

        BoxCastCollision();

        /*float footPosition = (bc.center.y + transform.position.y) - bc.size.y/2;
        float yMovement = 0;
        */
        
        
        /*
        if (groundDistance > footPosition && groundDistance != 1000)
            yMovement = groundDistance - footPosition;
        else if (footPosition - groundDistance < _fallSpeed && groundDistance != 1000)
            yMovement = -(footPosition - groundDistance);
        else
            yMovement = -_fallSpeed;

        if (yMovement > 0)
        {
            int i = 0;
        }*/

        if (objectSlantAngle > 45)
        {
            angleFromObject = (angleFromObject - 180) * Mathf.PI / 180;
            
            Vector3 angleHit = new Vector3( transform.forward.x * Mathf.Cos(angleFromObject) + transform.forward.z * Mathf.Sin(angleFromObject),
                transform.forward.y,
                -transform.forward.x * Mathf.Sin(angleFromObject) + transform.forward.z * Mathf.Cos(angleFromObject));  //rotating forward vector around angleFromObject

            movement = movement - Vector3.Project(movement, angleHit);
        }

        if (!isGrounded)
        {
            currentGravity = Physics.gravity;
        }
        else
        {

            
            currentGravity = -groundNormal * Physics.gravity.magnitude;
            
            if (rb.velocity.y >0)
            {
                rb.velocity = Vector3.zero;
            }
        }
        
        rb.AddForce(currentGravity, ForceMode.Acceleration);
        transform.position += (movement);
        
        

    }

    void OnCollisionStay(Collision ourCollision)
    {
        isGrounded = CheckGrounded(ourCollision);
    }

    private void OnCollisionExit(Collision other)
    {
        isGrounded = false;
        groundNormal = new Vector3();
    }

    bool CheckGrounded(Collision newCol)
    {
        cPoints = new ContactPoint[newCol.contactCount];
        newCol.GetContacts(cPoints);
        foreach (ContactPoint cP in cPoints)
        {
            //Debug.LogError(Vector3.Angle(cP.normal, -Physics.gravity.normalized));
            
            if (maxGroundedAngle > Vector3.Angle(cP.normal, -Physics.gravity.normalized))
            {
                groundNormal = cP.normal;
                return true;
            }
            else
            {
                //Debug.LogError(Vector3.Angle(cP.normal, -Physics.gravity.normalized));
            }
        }

        return false;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void BoxCastCollision()
    {

        RaycastHit hit;

        if (Physics.BoxCast(bc.transform.position + bc.center,
            new Vector3(bc.size.x/2, bc.size.y/2, bc.size.z/2), transform.forward, out hit,
            Quaternion.identity, 1))
        {

            if (hit.transform.gameObject.CompareTag("Solid"))
            {
                objectSlantAngle = Vector3.Angle(hit.normal, -Physics.gravity.normalized);
                angleFromObject =  Vector3.Angle(hit.normal, transform.forward);
                //also find angle between point and player
            }
            else
            {
                objectSlantAngle = 0;
            }

        }else
        {
            objectSlantAngle = 0;
        }
    }

}
