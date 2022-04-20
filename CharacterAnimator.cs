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

	private float timer = 0.0f;
    private const float MULTIPLE_90= 90;
	private const float CONST = 0.5f;
	private const string HEAD_JOINT = "Head";
	private const int HEAD_SCALE = 8;
	private const int JOINT_SCALE = 2;
	private const int DEFAULT_VALUE = 0;
	private const int X_CORD = 0;
	private const int Y_CORD = 1;
	private const int Z_CORD = 2;

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
		float anglex = (Mathf.Atan2(u[Y_CORD], u[Z_CORD]))*Mathf.Rad2Deg; //converts to degrees
		float thetax = MULTIPLE_90- anglex;
		Matrix4x4 rx = MatrixUtils.RotateX(-thetax);
        Matrix4x4 rxInv = rx.inverse;

        float anglez = (Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(u[Y_CORD],2) +Mathf.Pow( u[Z_CORD],2)), u[X_CORD]))*Mathf.Rad2Deg;
		float thetaz = MULTIPLE_90 - anglez;
		Matrix4x4 rz = MatrixUtils.RotateZ(thetaz);
        Matrix4x4 rzInv = rz.inverse;


        return  rxInv * rzInv;
	}

	// Creates a Cylinder GameObject between two given points in 3D space
	GameObject CreateCylinderBetweenPoints(Vector3 p1, Vector3 p2, float diameter)
	{
		GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		// we tanslate the bone right in the middle of two joints
		Matrix4x4 t = MatrixUtils.Translate(new Vector3(CONST * (p1[X_CORD] + p2[X_CORD]), CONST * (p1[Y_CORD] + p2[Y_CORD]), CONST* (p1[Z_CORD] + p2[Z_CORD])));
		Matrix4x4 r = RotateTowardsVector(p2-p1);
		//the bone length is the distance between p1 and p2 and 1\const is the default height
		Matrix4x4 s = MatrixUtils.Scale(new Vector3(diameter, Vector3.Distance(p1, p2)*CONST, diameter));
		MatrixUtils.ApplyTransform(cylinder, t*r*s);
		return cylinder;
		
	}

	// Creates a GameObject representing a given BVHJoint and recursively creates GameObjects for it's child joints
	GameObject CreateJoint(BVHJoint joint, Vector3 parentPosition)
	{
		joint.gameObject = new GameObject(joint.name);
		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.parent = joint.gameObject.transform;
		Matrix4x4 s;
		Matrix4x4 t = MatrixUtils.Translate(parentPosition + joint.offset);
		if (joint.name == HEAD_JOINT)
		{
			s = MatrixUtils.Scale(new Vector3(HEAD_SCALE, HEAD_SCALE, HEAD_SCALE));
		}
		else
		{
			s = MatrixUtils.Scale(new Vector3(JOINT_SCALE, JOINT_SCALE, JOINT_SCALE));

		}
		MatrixUtils.ApplyTransform(sphere, s);
		MatrixUtils.ApplyTransform(joint.gameObject, t);
		
	   	//todo the root node's offset is ignored 

		if (!joint.isEndSite)
		{
			foreach (BVHJoint item in joint.children)
			{

				CreateJoint(item, parentPosition + joint.offset);
				GameObject bone = CreateCylinderBetweenPoints(parentPosition + joint.offset, joint.offset + parentPosition+item.offset, CONST);
				bone.transform.parent = joint.gameObject.transform;
			}
		}

		return joint.gameObject;
	}



	// Transforms BVHJoint according to the keyframe channel data, and recursively transforms its children
	private void TransformJoint(BVHJoint joint, Matrix4x4 parentTransform, float[] keyframe)
	{

		int rxc = joint.rotationChannels.x;
		int ryc = joint.rotationChannels.y;
		int rzc = joint.rotationChannels.z;

		Matrix4x4 rx = MatrixUtils.RotateX(keyframe[rxc]);
		Matrix4x4 ry = MatrixUtils.RotateY(keyframe[ryc]);
		Matrix4x4 rz = MatrixUtils.RotateZ(keyframe[rzc]);
		Matrix4x4 t;

		if (joint == data.rootJoint)
		{
			t = MatrixUtils.Translate(new Vector3(keyframe[joint.positionChannels.x], keyframe[joint.positionChannels.y], keyframe[joint.positionChannels.z]));
		}
		else
		{
			t = MatrixUtils.Translate(new Vector3(joint.offset[X_CORD],joint.offset[Y_CORD],joint.offset[Z_CORD]));
		}

		//creates a list of matrices and adds the rotation matrices according to the given order and then we multiply them from left to right
        List<Matrix4x4> ordered_rotations = new List<Matrix4x4> {Matrix4x4.identity, Matrix4x4.identity, Matrix4x4.identity };
		ordered_rotations[joint.rotationOrder.x] = rx;
        ordered_rotations[joint.rotationOrder.y] = ry;
        ordered_rotations[joint.rotationOrder.z] = rz;

        Matrix4x4 r = ordered_rotations[0] * ordered_rotations[1] * ordered_rotations[2];
        
        // scale is not needed S=I4
		Matrix4x4 local_M = t*r;
		Matrix4x4 global_M = parentTransform * local_M;
		MatrixUtils.ApplyTransform(joint.gameObject, global_M );

		if (!joint.isEndSite)
		{
			foreach (BVHJoint item in joint.children)
			{

				TransformJoint(item, global_M, keyframe);
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (animate)
		{
			if (timer >= data.frameLength)
			{
				if (currFrame < data.numFrames - 1)

				{

					TransformJoint(data.rootJoint, Matrix4x4.identity, data.keyframes[currFrame]);
					currFrame++;

				}
				else
				{
					currFrame = DEFAULT_VALUE;
				}
				timer = DEFAULT_VALUE;

			}
			timer = timer + Time.deltaTime;

		}
	}
}
