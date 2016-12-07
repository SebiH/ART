#pragma once

#include <string>

// micro optimisation
#define MAX_MARKERS 8

namespace Optitrack
{
	struct Vector3
	{
		float X;
		float Y;
		float Z;
	};

	struct Quaternion
	{
		float X;
		float Y;
		float Z;
		float W;
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

		Marker Markers[MAX_MARKERS];

		Rigidbody(const int id, const std::string &name, const Vector3 &pos, const Quaternion &rot)
			: Id(id), Name(name), Position(pos), Rotation(rot)
		{ }

		void AddMarker(int id, const Vector3 &pos)
		{
			if (id < MAX_MARKERS)
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
