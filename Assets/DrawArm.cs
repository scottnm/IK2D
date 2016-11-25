using UnityEngine;
using System.Collections;

namespace Filibusters
{
    public class DrawArm : MonoBehaviour
    {
        GameObject Shoulder;
        GameObject UpperArmEnd;
        GameObject LowerArmEnd;

        LineRenderer lr;

        void Start()
        {
            Shoulder = GameObject.Find("Shoulder"); 
            UpperArmEnd = GameObject.Find("UpperArmEnd"); 
            LowerArmEnd = GameObject.Find("LowerArmEnd");

            lr = GetComponent<LineRenderer>();
        }

        void Update()
        {
            lr.SetPosition(0, Shoulder.transform.position);
            lr.SetPosition(1, UpperArmEnd.transform.position);
            lr.SetPosition(2, LowerArmEnd.transform.position);
        }
    }
}
