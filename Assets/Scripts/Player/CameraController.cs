using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{

    [SerializeField] Transform player;
    [SerializeField] Transform deformTransform;
    [SerializeField] Transform[] deformPositions = new Transform[3];
    PlayerController pc;
    [SerializeField] float cameraDistance = 7;
    [SerializeField] float heightOffset;
    [SerializeField] float tightness;
    [SerializeField] float collisionTightness;

    [SerializeField] float vertSensitivity;
    [SerializeField] float horSensitivity;
    [SerializeField] float cameraRayThickness;

    Vector3 localCameraDefaultPos;

    Vector2 mouseDelta;

    [HideInInspector] public bool lockOnTarget;
    //public Transform target;
    //Transform orb;
    Transform focusTransform;
    Transform _camera;
    Transform cameraTarget;

    public bool takeInput = true;


    void OnEnable()
    {
        pc = player.GetComponent<PlayerController>();
        focusTransform = transform.GetChild(0);
        _camera = transform.GetChild(0).GetChild(0);
        _camera.localPosition = new Vector3(0, 0, -cameraDistance);
        _camera.rotation = Quaternion.LookRotation(focusTransform.position - _camera.position);

        localCameraDefaultPos = _camera.localPosition;

        cameraTarget = transform.GetChild(1);
        cameraTarget.position = _camera.position;
        lockOnTarget = true;
    }

    void Update()
    {
        float cameraDot = Vector3.Dot(_camera.forward, Vector3.up);

        if (cameraDot < -0.3f)
        {
            deformTransform.localPosition = deformPositions[0].localPosition;
        }
        else if (cameraDot < 0.2f)
        {
            deformTransform.localPosition = deformPositions[1].localPosition;
        }
        else
        {
            deformTransform.localPosition = deformPositions[2].localPosition;
        }
    }

    void LateUpdate()
    {
        mouseDelta = pc.mouseDelta;

        transform.Rotate(new Vector3(0, mouseDelta.x * horSensitivity, 0), Space.World);

        float vertSenseModifier = 0.6f;

        float newXRot = focusTransform.localEulerAngles.x - mouseDelta.y * vertSensitivity * vertSenseModifier;

        if (newXRot < 75 || newXRot > 300)
        {

            focusTransform.localEulerAngles = new Vector3(newXRot, focusTransform.localEulerAngles.y, 0);
        }

        // Stops the camera from clipping under the floor while still allowing the player to look upwards
        //if (player.position.y - cameraTarget.position.y < 0.2f)
        //    _camera.position = Vector3.up * 0.2f + Vector3.ProjectOnPlane(cameraTarget.position, Vector3.up) 
        //        + (Vector3.ProjectOnPlane(player.position - cameraTarget.position, Vector3.up)) * (-0.2f * (cameraTarget.position.y - 0.2f));

        transform.position = Vector3.Lerp(transform.position, player.position, tightness * 60 * Time.deltaTime);


        
        if (newXRot > 290 && newXRot < 345)
        {
            float lerp = Mathf.Lerp(0, 1f, (45 - (newXRot - 300)) / 45);
            lerp = lerp * lerp;
            
            focusTransform.localPosition = new Vector3(focusTransform.localPosition.x, heightOffset * (1 + lerp), focusTransform.localPosition.z);
        }
        else
        {
            focusTransform.localPosition = new Vector3(focusTransform.localPosition.x, heightOffset * 1, focusTransform.localPosition.z);
        }

        List<Vector3> offsetVectors = new List<Vector3>();
        offsetVectors.Add(Vector3.zero);
        offsetVectors.Add(Quaternion.AngleAxis(45, _camera.forward) * _camera.up * cameraRayThickness);
        offsetVectors.Add(Quaternion.AngleAxis(135, _camera.forward) * _camera.up * cameraRayThickness);
        offsetVectors.Add(Quaternion.AngleAxis(225, _camera.forward) * _camera.up * cameraRayThickness);
        offsetVectors.Add(Quaternion.AngleAxis(315, _camera.forward) * _camera.up * cameraRayThickness);
        
        Vector3 rayPointLocal = localCameraDefaultPos;
        float minDist = cameraDistance;
        
        for (int i = 0; i < 5; i++)
        {
        
            Vector3 cameraFocusPoint = focusTransform.position + offsetVectors[i];
        
        
            RaycastHit hit;
            if (Physics.Raycast(cameraFocusPoint, ((_camera.position + offsetVectors[i]) - cameraFocusPoint).normalized, out hit, cameraDistance) && hit.transform.CompareTag("Terrain"))
            {
                if ((cameraFocusPoint - hit.point).magnitude < minDist)
                {
                    minDist = (cameraFocusPoint - hit.point).magnitude;
                    rayPointLocal = focusTransform.worldToLocalMatrix.MultiplyPoint(hit.point - offsetVectors[i]);
                }
            }
        }
        
        _camera.localPosition = Vector3.Lerp(_camera.localPosition, rayPointLocal, Time.deltaTime * collisionTightness);


    }
}
