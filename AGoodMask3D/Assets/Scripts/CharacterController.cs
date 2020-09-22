using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.XR;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float _camDefaultHeight = -2.7f;
    [SerializeField] private float speed = 10;
    private bool _walking = false;
    private float _zDirection = 0;
    private float _xDirection = 0;

    private Vector3 lastSpot;
    
    public static class AxisInput {
        public const string LeftHorizontal = "Horizontal";
        public const string LeftVertical = "Vertical";
        public const string RightHorizontal = "RHorizontal";
        public const string RightVertical = "RVertical";
    }
    
    private void FixedUpdate()
    {
        
        Movement();
        AdjustCamera();
    }

    private void AdjustCamera()
    {
        
        var posZ = new Vector3(transform.forward.x * -5.5f,2f,transform.forward.z * -5.5f); // THIS -5 NEEDS TO BE ADJUSTABLE WITH R STICK
        cam.transform.position = transform.position + (posZ);

        var xDiff = transform.position.x - cam.transform.position.x;
        var zDiff = transform.position.z - cam.transform.position.z;

        var newAngle = Mathf.Atan(xDiff / zDiff) * 180/Mathf.PI;

        if (zDiff <= 0)
            newAngle = newAngle + 180;

        var currentAngle = cam.transform.eulerAngles.y;
        var angleDiff = newAngle - currentAngle;

        if (angleDiff > 200)
            angleDiff -= 360;
        else if (angleDiff < -200)
            angleDiff += 360;

        if (currentAngle == newAngle)
            lastSpot = transform.position;

        Debug.LogError(lastSpot);
        if(Vector3.Distance(lastSpot, transform.position) > 2) //2 is the distance you need to move beforte the camera starts following you
            cam.transform.eulerAngles = new Vector3(0, 
            ((angleDiff/20)>newAngle) ? (newAngle) : (currentAngle +angleDiff / 20), 0); //20 is the number for how slow it pans back to the player
        
       
           
    }

    private void Movement(){

        _xDirection = Input.GetAxis(AxisInput.LeftHorizontal);
        _zDirection = Input.GetAxis(AxisInput.LeftVertical);

        if (_xDirection != 0 || _zDirection != 0)
            _walking = false;
        else
            _walking = true;
        
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

        var moveX = _xDirection * speed * new Vector3(cam.transform.right.x,0,cam.transform.right.z);
        var moveZ = _zDirection * speed * new Vector3(cam.transform.forward.x,0,cam.transform.forward.z);
        var movement = moveX + moveZ;
            
        movement *= Time.deltaTime;

        if(_xDirection != 0 || _zDirection !=0){
            transform.rotation = Quaternion.LookRotation(movement);
            //transform.eulerAngles = new Vector3(0,transform.eulerAngles.y,0);
        }
        transform.position += movement;
    }

}
