using UnityEngine;
using System.Collections;

public class IKArm : MonoBehaviour
{
    private class JointAngleChunk
    {
        public Vector3 pos;
        public float angle;
        public float oppositeSideLen;
    }

    enum Joint
    {
        Shoulder = 0,
        Elbow = 1,
        Hand = 2
    }

    private readonly float HALF_PI = Mathf.PI / 2f;


    [SerializeField]
    float forearmLength;
    [SerializeField]
    float lowerarmLength;
    float armSpan;
    JointAngleChunk [] IKChunks;
    LineRenderer lr;
    Camera cam;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        cam = GameObject.FindObjectOfType<Camera>();
        armSpan = forearmLength + lowerarmLength;
        InitIkData();
    }

    void InitIkData()
    {
        IKChunks = new JointAngleChunk[3];
        for (int i = 0; i < 3; ++i)
        {
            IKChunks[i] = new JointAngleChunk();
        }

        var shoulder = getIKChunk(Joint.Shoulder);
        shoulder.pos = transform.position;
        shoulder.angle = 0;
        shoulder.oppositeSideLen = lowerarmLength;

        var elbow = getIKChunk(Joint.Elbow);
        elbow.pos = transform.position;
        elbow.pos.x += forearmLength;
        elbow.angle = HALF_PI;
        elbow.oppositeSideLen = lowerarmLength + forearmLength;

        var hand = getIKChunk(Joint.Hand);
        hand.pos = elbow.pos;
        hand.pos.x += lowerarmLength;
        hand.angle = 0;
        hand.oppositeSideLen = forearmLength;
    }

    void Update()
    {
        CalculateHand();
        IkAngleSolver();
        IkCalculateElbow();
        DrawArm();

        DebugIkChunks();
    }

    void DebugIkChunks()
    {

        Debug.Log(string.Format("{0, 10} | {1, 20} | {2, 20} | {3, 20}\n", "        ", "Pos", "Angle", "OppSideLen"));
        Debug.Log(string.Format("{0, 10} | {1, 20} | {2, 20} | {3, 20}\n", "Shoulder", IKChunks[0].pos, 180f/Mathf.PI * IKChunks[0].angle, IKChunks[0].oppositeSideLen));
        Debug.Log(string.Format("{0, 10} | {1, 20} | {2, 20} | {3, 20}\n", "Elbow", IKChunks[1].pos, 180f/Mathf.PI * IKChunks[1].angle, IKChunks[1].oppositeSideLen));
        Debug.Log(string.Format("{0, 10} | {1, 20} | {2, 20} | {3, 20}\n", "Hand", IKChunks[2].pos, 180f/Mathf.PI * IKChunks[2].angle, IKChunks[2].oppositeSideLen));
        Debug.DrawLine(transform.position, getIKChunk(Joint.Hand).pos, Color.green);
    }

    void DrawArm()
    {
        lr.SetPositions(new Vector3[] {
            transform.position,
            getIKChunk(Joint.Elbow).pos,
            getIKChunk(Joint.Hand).pos });
    }

    void IkAngleSolver()
    {
        // Update the side length from the shoulder to the hand
        getIKChunk(Joint.Elbow).oppositeSideLen =
            (getIKChunk(Joint.Hand).pos - transform.position).magnitude;

        // find largest side
        int maxSideIndex = -1;
        float maxSide = -1;
        for (int i = 0; i < IKChunks.Length; ++i)
        {
            if (IKChunks[i].oppositeSideLen > maxSide)
            {
                maxSideIndex = i;
                maxSide = IKChunks[i].oppositeSideLen;
            }
        }

        float aSide = 0f, bSide = 0f, cSide = 0f;
        switch ((Joint)maxSideIndex)
        {
            case Joint.Shoulder:
                aSide = getIKChunk(Joint.Shoulder).oppositeSideLen;
                bSide = getIKChunk(Joint.Elbow).oppositeSideLen;
                cSide = getIKChunk(Joint.Hand).oppositeSideLen;
                break;
            case Joint.Elbow:
                aSide = getIKChunk(Joint.Elbow).oppositeSideLen;
                bSide = getIKChunk(Joint.Hand).oppositeSideLen;
                cSide = getIKChunk(Joint.Shoulder).oppositeSideLen;
                break;
            case Joint.Hand:
                aSide = getIKChunk(Joint.Elbow).oppositeSideLen;
                bSide = getIKChunk(Joint.Hand).oppositeSideLen;
                cSide = getIKChunk(Joint.Shoulder).oppositeSideLen;
                break;
        }
        float maxSideAngle = triangleCosSolve(aSide, bSide, cSide);
        IKChunks[maxSideIndex].angle = maxSideAngle;

        int nextSideIndex = (maxSideIndex + 1) % 3;
        float nextSideAngle = triangleSinSolve(
            IKChunks[maxSideIndex].oppositeSideLen,
            IKChunks[maxSideIndex].angle,
            IKChunks[nextSideIndex].oppositeSideLen);
        IKChunks[nextSideIndex].angle = nextSideAngle;

        int finalSideIndex = (nextSideIndex + 1) % 3;
        IKChunks[finalSideIndex].angle = Mathf.PI - nextSideAngle - maxSideAngle;

        Debug.Log(string.Format("CalculatedAngles: {0}, {1}, {2}\n\n", maxSideIndex, nextSideIndex, finalSideIndex));
    }

    void IkCalculateElbow()
    {
        /*
         * Naming conventions:
         * Let vector from shoulder to hand be: VSH
         */

        /* Get angle between the horizontal and the vector that connects the shoulder
         * to the IK Hand
         * 
         * Normally to get the angle between to vectors you need to do...
         * acos ( dot(v1, v2) / (mag(v1) * mag(v2)) )
         * since we know the length of our horizontal is 1 this becomes...
         * acos ( dot(v1, v2) / (mag(v2) )
         * and since we know the dot product of the unit right vector and any other vector
         * is just the x become this becomes...
         * acos (v2.x / mag(v2))
         */
        var SHvec = getIKChunk(Joint.Hand).pos - transform.position;
        var SHside = getIKChunk(Joint.Elbow).oppositeSideLen;
        var VSHangle = SHvec.y >= 0 ? Mathf.Acos( SHvec.x / SHside ) : -Mathf.Acos( SHvec.x / SHside );

        //rcostheta, rsintheta
        var SEside = getIKChunk(Joint.Hand).oppositeSideLen;
        var SEangle = VSHangle - getIKChunk(Joint.Shoulder).angle;
        var SEvec = new Vector3(Mathf.Cos(SEangle), Mathf.Sin(SEangle)) * SEside;

        getIKChunk(Joint.Elbow).pos = transform.position + SEvec;
    }

    float triangleCosSolve(float aSide, float bSide, float cSide)
    {
        return Mathf.Acos(
                ((bSide * bSide) + (cSide * cSide) - (aSide * aSide)) / (2 * bSide * cSide)
            );
    }

    float triangleSinSolve(float aSide, float aAngle, float bSide)
    {
        return Mathf.Asin( bSide * Mathf.Sin(aAngle) / aSide );
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

        getIKChunk(Joint.Hand).pos = shoulderToHand + transform.position;
    }

    private JointAngleChunk getIKChunk(Joint j)
    {
        return IKChunks[(int)j];
    }
}