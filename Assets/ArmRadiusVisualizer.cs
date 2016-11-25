using UnityEngine;
using System.Collections;

namespace Filibusters
{
    public class ArmRadiusVisualizer : MonoBehaviour
    {
        void Start()
        {
            float radialScale = 1f / GetComponent<CircleCollider2D>().radius;
            GameObject Shoulder = GameObject.Find("Shoulder"); 
            GameObject UpperArmEnd = GameObject.Find("UpperArmEnd"); 
            GameObject LowerArmEnd = GameObject.Find("LowerArmEnd");

            float armLength =
                Vector3.Distance(LowerArmEnd.transform.position, UpperArmEnd.transform.position) +
                Vector3.Distance(UpperArmEnd.transform.position, Shoulder.transform.position);

            radialScale *= armLength;

            transform.localScale = new Vector3(radialScale, radialScale, 1);
        }
    }
}
