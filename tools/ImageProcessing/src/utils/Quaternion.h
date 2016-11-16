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
		void RotMatToQuaternion(Quaternion *outQuat, const float *inMat);
		Quaternion MultiplyQuaternion(Quaternion *a, Quaternion *b);
	};
}
