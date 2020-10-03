using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private float _minInteractDistance = 10;
    [SerializeField] private float _lockedTargetExitDistance = 15;
    [SerializeField] private float _yOffset = 5f;

    private GameObject player;
    private CameraControllerFollow camera;

    private bool isLockedOn = false;
    private float currentDistanceFromPlayer = 10000;
    private bool closeEnough = false;
    private bool addedToList = false;

    public static class AxisInput {
        public const string LEFT_TRIGGER = "LTrigger";
    }
    
    public bool IsLockedOn
    {
        get => isLockedOn;
        set => isLockedOn = value;
    }

    public bool CloseEnough => closeEnough;

    public float YOffset => _yOffset;

    public float CurrentDistanceFromPlayer => currentDistanceFromPlayer;
    

    public bool AddedToList
    {
        set => addedToList = value;
    }


    void Start()
    {
        camera = Camera.main.GetComponent<CameraControllerFollow>();
        player = camera.Player;
    }

    void Update()
    {
        currentDistanceFromPlayer = Vector3.Distance(transform.position, player.transform.position);
        
        if (currentDistanceFromPlayer <= _minInteractDistance)
        {
            closeEnough = true;
            if(!addedToList)
            {
                camera.AddInteractableToList(this);
                addedToList = true;
            }
        }
        else
        {
            addedToList = false;
            closeEnough = false;
        }

        if (isLockedOn && (Input.GetAxis(AxisInput.LEFT_TRIGGER) == 0 || currentDistanceFromPlayer > _lockedTargetExitDistance))
        {
            isLockedOn = false;
        }

    }
    
}
