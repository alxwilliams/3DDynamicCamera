using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private float _minInteractDistance;

    private GameObject player;
    private CameraControllerFollow camera;

    private float currentDistanceFromPlayer = 10000;
    private bool closeEnough = false;
    private bool addedToList = false;
    
    public bool CloseEnough => closeEnough; 
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
        
    }
    
}
