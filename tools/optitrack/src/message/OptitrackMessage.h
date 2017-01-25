#pragma once

#include <string>

namespace Optitrack
{
	// micro optimisation
	const int Max_Markers = 8;

	struct Vector3
	{
		float X;
		float Y;
		float Z;

		Vector3() {}
		Vector3(float x, float y, float z)
			: X(x), Y(y), Z(z) {}
	};

	struct Quaternion
	{
		float X;
		float Y;
		float Z;
		float W;

		Quaternion() {}
		Quaternion(float x, float y, float z, float w)
			: X(x), Y(y), Z(z), W(w) {}
	};

	struct Marker
	{
		bool IsValid = false;
		int Id;
		Vector3 Position;
	};

	class Rigidbody
	{
	public:
		int Id;
		std::string Name;
		Vector3 Position;
		Quaternion Rotation;

		Marker Markers[Max_Markers];

		Rigidbody(const int id, const std::string &name, const Vector3 &pos, const Quaternion &rot)
			: Id(id), Name(name), Position(pos), Rotation(rot)
		{ }

		void AddMarker(int id, const Vector3 &pos)
		{
			if (id < Max_Markers)
			{
				Markers[id].IsValid = true;
				Markers[id].Id = id;
				Markers[id].Position = pos;
			}
			else
			{
				throw std::exception("Too many markers!");
			}
		}
	};


	class OptitrackMessage
	{
	public:
		OptitrackMessage() {}
		virtual ~OptitrackMessage() {}

		virtual void AddRigidbody(const Rigidbody &body) = 0;
		virtual const char* Print() = 0;
	};
}
