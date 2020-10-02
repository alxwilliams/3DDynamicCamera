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
    [SerializeField] private float speed = 10;
    private bool _usingKeyboard = false;
    private bool _rightArrow = false;
    private bool _leftArrow = false;
    private bool _upArrow = false;
    private bool _downArrow = false;
    private bool _walking = false;
    private float _zDirection = 0;
    private float _xDirection = 0;

    private Vector3 lastSpot;
    
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

        if (_xDirection != 0 || _zDirection != 0)
            _walking = true;
        else
            _walking = false;
        
            //AnimationCheck();
        
        if (_xDirection != 0 && _zDirection != 0)
        {
            _xDirection = _xDirection * Mathf.Acos(45 * Mathf.PI/180);
            _zDirection = _zDirection * Mathf.Acos(45 * Mathf.PI/180);
        }

        /*anim.SetFloat("VerticalLast",_verticalLast);
        anim.SetFloat("HorizontalLast",_horizontalLast);
        anim.SetBool("Walking",_walking);*/
        
        //RayCastCollision();

        if (!_walking)
        {
            anim.SetFloat("Speed",0f);
        }
        else
        {
            anim.SetFloat("Speed", 5.0f);
        }

        var moveX = _xDirection * speed * new Vector3(cam.transform.right.x,0,cam.transform.right.z);
        var moveZ = _zDirection * speed * new Vector3(cam.transform.forward.x,0,cam.transform.forward.z);
        var movement = moveX + moveZ;
            
        movement *= Time.deltaTime;

        
        if(_xDirection != 0 || _zDirection !=0)
        {
            if(Input.GetAxis(AxisInput.LEFT_TRIGGER) == 0) //make player face target if player here instead of no turning
                transform.rotation = Quaternion.LookRotation(movement); //will have to start strafe animation here
        }
        transform.position += movement;
    }

}
