using UnityEngine;
using System.Collections;

public class IKArm : MonoBehaviour
{
    [SerializeField]
    float forearmLength;
    [SerializeField]
    float lowerarmLength;
    float armSpan;

    Vector3 ElbowPos;
    Vector3 HandPos;

    LineRenderer lr;

    Camera cam;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        cam = GameObject.FindObjectOfType<Camera>();
        armSpan = forearmLength + lowerarmLength;
        ElbowPos = transform.position;
        ElbowPos.x = ElbowPos.x + forearmLength;
        HandPos = ElbowPos;
        HandPos.x = HandPos.x + forearmLength;
    }

    void Update()
    {
        CalculateHand();
        DrawArm();
    }

    void DrawArm()
    {
        lr.SetPositions(new Vector3[] {
            transform.position,
            ElbowPos,
            HandPos });

        Debug.DrawLine(transform.position, HandPos, Color.green);
    }

    void CalculateHand()
    {
        var worldSpaceMouse = cam.ScreenToWorldPoint(Input.mousePosition);
        worldSpaceMouse.z = 0;
        var shoulderToHand = worldSpaceMouse - transform.position;
        if (shoulderToHand.magnitude > armSpan)
        {
            shoulderToHand = shoulderToHand.normalized * armSpan;
        }

        HandPos = shoulderToHand + transform.position;
    }
}