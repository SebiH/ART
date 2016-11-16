#include <math.h>
#include "utils/Quaternion.h"

using namespace ImageProcessing;

// Adapted from OVRVision
void Quaternion::RotMatToQuaternion(Quaternion * outQuat, const float * inMat)
{
	float s;
	float tr = inMat[0] + inMat[5] + inMat[10] + 1.0f;
	if (tr >= 1.0f) {
		s = 0.5f / sqrtf(tr);
		outQuat->w = 0.25f / s;
		outQuat->x = (inMat[6] - inMat[9]) * s;
		outQuat->y = (inMat[8] - inMat[2]) * s;
		outQuat->z = (inMat[1] - inMat[4]) * s;
		return;
	}
	else {
		float max;
		max = inMat[5] > inMat[10] ? inMat[5] : inMat[10];

		if (max < inMat[0]) {
			s = sqrtf(inMat[0] - inMat[5] - inMat[10] + 1.0f);
			float x = s * 0.5f;
			s = 0.5f / s;
			outQuat->x = x;
			outQuat->y = (inMat[1] + inMat[4]) * s;
			outQuat->z = (inMat[8] + inMat[2]) * s;
			outQuat->w = (inMat[6] - inMat[9]) * s;
			return;
		}
		else if (max == inMat[5]) {
			s = sqrtf(-inMat[0] + inMat[5] - inMat[10] + 1.0f);
			float y = s * 0.5f;
			s = 0.5f / s;
			outQuat->x = (inMat[1] + inMat[4]) * s;
			outQuat->y = y;
			outQuat->z = (inMat[6] + inMat[9]) * s;
			outQuat->w = (inMat[8] - inMat[2]) * s;
			return;
		}
		else {
			s = sqrtf(-inMat[0] - inMat[5] + inMat[10] + 1.0f);
			float z = s * 0.5f;
			s = 0.5f / s;
			outQuat->x = (inMat[8] + inMat[2]) * s;
			outQuat->y = (inMat[6] + inMat[9]) * s;
			outQuat->z = z;
			outQuat->w = (inMat[1] - inMat[4]) * s;
			return;
		}
	}

}

Quaternion Quaternion::MultiplyQuaternion(Quaternion * a, Quaternion * b)
{
	Quaternion ans;
	ans.w = a->w * b->w - a->x * b->x - a->y * b->y - a->z * b->z;
	ans.x = a->w * b->x + b->w * a->x + a->y * b->z - b->y * a->z;
	ans.y = a->w * b->y + b->w * a->y + a->z * b->x - b->z * a->x;
	ans.z = a->w * b->z + b->w * a->z + a->x * b->y - b->x * a->y;
	return ans;
}
