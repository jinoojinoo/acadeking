using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fov : MonoBehaviour
{
    //     private Camera cam;
    //     private float horizontalFOV = 0;
    //     private float loggedFOV = 0;
    // #if UNITY_EDITOR
    //     public void OnDrawGizmos()
    //     {
    //         cam = Camera.main;
    //         if (cam != null)
    //         {
    //             horizontalFOV = getHorizontalAngle(cam);
    //             if (horizontalFOV != loggedFOV)
    //             {
    //                 loggedFOV = horizontalFOV;
    //                 logFOV(horizontalFOV);
    //             }
    //         }
    //     }
    //     private float getHorizontalAngle(Camera camera)
    //     {
    //         float vFOVrad = camera.fieldOfView * Mathf.Deg2Rad;
    //         float cameraHeightAt1 = Mathf.Tan(vFOVrad * .5f);
    //         return Mathf.Atan(cameraHeightAt1 * camera.aspect) * 2f * Mathf.Rad2Deg;
    //     }
    // 
    //     void OnEnable()
    //     {
    //         if (loggedFOV != 0)
    //         {
    //             logFOV(loggedFOV);
    //         }
    //     }
    //     private void logFOV(float value)
    //     {
    //         Debug.Log(name + "FOV: " + value);
    //     }
    // #endif

    public LineRenderer lineRenderer;
    public float rotationSpeed = 4.04f;

    Vector3 center = Vector3.zero;
    Vector3 theArc = Vector3.zero;

    void Start()
    {
        lineRenderer.SetColors(Color.red, Color.red);
        lineRenderer.SetWidth(0.1f, 0.1f);
        lineRenderer.SetVertexCount(25);
    }

    void Update()
    {
        if (null == Camera.main)
            return;

        // Generate a plane that intersects the transform's position with an upwards normal.
        Plane playerPlane = new Plane(Vector3.up, transform.position);

        // Generate a ray from the cursor position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Determine the point where the cursor ray intersects the plane.
        // This will be the point that the object must look towards to be looking at the mouse.
        // Raycasting to a Plane object only gives us a distance, so we'll have to take the distance,
        //   then find the point along that ray that meets that distance.  This will be the point
        //   to look at.
        float hitdist = 0.0f;

        // If the ray is parallel to the plane, Raycast will return false.
        Vector3 targetPoint = Vector3.zero;

        if (playerPlane.Raycast(ray, out hitdist))
        {
            // Get the point along the ray that hits the calculated distance.          
            targetPoint = ray.GetPoint(hitdist);

            // Draw the arc trajectory     
            center = (transform.position + targetPoint) * 0.5f;
            center.y -= 70.0f;

            // Determine the target rotation.  This is the rotation if the transform looks at the middle between the target object and your object.

            Quaternion targetRotation = Quaternion.LookRotation(center - transform.position);

            // Smoothly rotate towards the target point.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // shorten the ray if it's hitting some obstacle:
            RaycastHit hitInfo;

            if (Physics.Linecast(transform.position, targetPoint, out hitInfo))
            {
                targetPoint = hitInfo.point;
            }
        }
        else
        {
            targetPoint = transform.position;
        }

        Vector3 RelCenter = transform.position - center;
        Vector3 aimRelCenter = targetPoint - center;

        // Draw the arc line starting from the launcher
        for (float index = 0.0f, interval = -0.0417f; interval < 1.0f;)
        {
            theArc = Vector3.Slerp(RelCenter, aimRelCenter, interval += 0.0417f);
            lineRenderer.SetPosition((int)index++, theArc + center);
        }
    }
}