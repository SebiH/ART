#pragma once

namespace ImageProcessing
{
	// Adapted from OVRVision
	struct Quaternion
	{
	public:
		float x;
		float y;
		float z;
		float w;

	public:
		static void RotMatToQuaternion(Quaternion *outQuat, const float *inMat);
		static Quaternion MultiplyQuaternion(Quaternion *a, Quaternion *b);
	};
}
