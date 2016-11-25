using UnityEngine;
using System.Collections;

namespace Filibusters
{
    public class ArmController : MonoBehaviour
    {
        Camera cam;
        Vector3 adjustedPos;
        GameObject Shoulder;
        float fullArmLength;
        void Start()
        {
            cam = GameObject.FindObjectOfType<Camera>();
            adjustedPos = new Vector3();
            Shoulder = GameObject.Find("Shoulder"); 
            GameObject UpperArmEnd = GameObject.Find("UpperArmEnd");

            fullArmLength =
                Vector3.Distance(transform.position, UpperArmEnd.transform.position) +
                Vector3.Distance(UpperArmEnd.transform.position, Shoulder.transform.position);
        }

        void Update()
        {
            adjustedPos = cam.ScreenToWorldPoint(Input.mousePosition);
            adjustedPos.z = 0;
            Debug.Log(adjustedPos);
            var ikCenter = Shoulder.transform.position;
            if ((adjustedPos - ikCenter).magnitude > fullArmLength)
            {
                adjustedPos = (adjustedPos - ikCenter).normalized * fullArmLength + ikCenter;
            }
            transform.position = adjustedPos;
        }
    }
}
