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
        

        var movement = new Vector3(transform.forward.x * -2,transform.position.y + 3,transform.forward.z * -2);
        cam.transform.position = transform.position + (movement);

        
        cam.transform.LookAt(transform.position);
        /*var point = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);//when using the right hand stick, slightly adjusting this position
        Vector3 direction = point;
        Quaternion toRotation = Quaternion.FromToRotation(cam.transform.forward, direction);
        cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, toRotation, 1 * Time.time);*/
       
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
        _walking = false;
    }

}
