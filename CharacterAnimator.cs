using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public TextAsset BVHFile; // The BVH file that defines the animation and skeleton
    public bool animate; // Indicates whether or not the animation should be running

    private BVHData data; // BVH data of the BVHFile will be loaded here
    private int currFrame = 0; // Current frame of the animation

    // Start is called before the first frame update
    void Start()
    {
        BVHParser parser = new BVHParser();
        data = parser.Parse(BVHFile);
        CreateJoint(data.rootJoint, Vector3.zero);
    }

    // Returns a Matrix4x4 representing a rotation aligning the up direction of an object with the given v
    Matrix4x4 RotateTowardsVector(Vector3 v)
    {
        Vector3 u = Vector3.Normalize(v);
        float anglex = Mathf.Atan2(u[1], u[2]); //convert to degrees
        float thetax = 90 - anglex;
        Matrix4x4 rx = MatrixUtils.RotateX(-thetax);
        Matrix4x4 rxInv = rx.inverse;

        float anglez = UnityEngine.Mathf.Atan2(Mathf.Sqrt(u[1] + u[2]), u[0]);
        float thetaz = 90 - anglez;
        Matrix4x4 rz = MatrixUtils.RotateZ(-thetaz);
        Matrix4x4 rzInv = rz.inverse;


        Matrix4x4 ry = MatrixUtils.RotateY(Vector3.Angle(new Vector3(0, 0, 0), v));

        //Vector3 up = new Vector3(0, 1, 0);
        //print((rxInv * rzInv * ry * rz * rx).MultiplyVector(up));


        return rxInv * rzInv * ry * rz * rx;
    }

    // Creates a Cylinder GameObject between two given points in 3D space
    GameObject CreateCylinderBetweenPoints(Vector3 p1, Vector3 p2, float diameter)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        Matrix4x4 t = MatrixUtils.Translate(new Vector3(0.5f * (p1[0] + p2[0]), 0.5f * (p1[1] + p2[1]), 0.5f * (p1[2] + p2[2])));
        Matrix4x4 r = MatrixUtils.RotateTowardsVector(p2-p1);
        Matrix4x4 s = MatrixUtils.Scale(new Vector3(diameter, Vector3.Distance(p1, p2)/2, diameter));
        MatrixUtils.ApplyTransform(cylinder, t*r*s);
        return cylinder;
        
    }

    // Creates a GameObject representing a given BVHJoint and recursively creates GameObjects for it's child joints
    GameObject CreateJoint(BVHJoint joint, Vector3 parentPosition)
    {
        GameObject g = new GameObject(joint.name);
        joint.gameObject = g;
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.parent = joint.gameObject.transform;
        Matrix4x4 s;
        Matrix4x4 t = MatrixUtils.Translate(parentPosition + joint.offset);
        if (joint.name == "Head")
        {
            s = MatrixUtils.Scale(new Vector3(8, 8, 8));


        }
        else
        {
            s = MatrixUtils.Scale(new Vector3(2, 2, 2));

        }
        MatrixUtils.ApplyTransform(joint.gameObject, t * s);
        float diameter = 0.5f;
        
       

        if (!joint.isEndSite)
        {
            foreach (BVHJoint item in joint.children)
            {

                CreateJoint(item, parentPosition + joint.offset);
                GameObject bone = CreateCylinderBetweenPoints(parentPosition + joint.offset, joint.offset + parentPosition+item.offset, diameter);
                bone.transform.parent = joint.gameObject.transform;
                Debug.DrawLine(parentPosition + joint.offset, joint.offset + parentPosition + item.offset, Color.red, 30, false);
            }
        }

        return g;
    }

    // Transforms BVHJoint according to the keyframe channel data, and recursively transforms its children
    private void TransformJoint(BVHJoint joint, Matrix4x4 parentTransform, float[] keyframe)
    {
        // Your code here
    }

    // Update is called once per frame
    void Update()
    {
        if (animate)
        {
            // Your code here
        }
    }
}
