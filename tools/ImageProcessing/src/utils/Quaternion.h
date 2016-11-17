#pragma once

namespace ImageProcessing
{
	// Adapted from OVRVision
	struct Quaternion
	{
		union
		{
			float v[4];
			struct
			{
				float x;
				float y;
				float z;
				float w;
			};
		};

	public:
		static void RotMatToQuaternion(Quaternion *outQuat, const float *inMat);
		static Quaternion MultiplyQuaternion(Quaternion *a, Quaternion *b);
	};
}
