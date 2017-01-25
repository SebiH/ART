#include "message/JsonOptitrackMessage.h"

using namespace Optitrack;


JsonOptitrackMessage::JsonOptitrackMessage()
	: is_sealed_(false),
	  writer_(buffer_)
{
	writer_.StartObject();
	writer_.String("Rigidbodies");
	writer_.StartArray();
}

JsonOptitrackMessage::~JsonOptitrackMessage()
{

}

void JsonOptitrackMessage::AddRigidbody(const Rigidbody &body)
{
	assert(is_sealed_ == false);

	writer_.StartObject();
	{
		writer_.String("Id");
		writer_.Int(body.Id);

		writer_.String("Name");
		writer_.String(body.Name);

		writer_.String("X");
		writer_.Double(body.Position.X);
		writer_.String("Y");
		writer_.Double(body.Position.Y);
		writer_.String("Z");
		writer_.Double(body.Position.Z);
		writer_.String("QX");
		writer_.Double(body.Rotation.X);
		writer_.String("QY");
		writer_.Double(body.Rotation.Y);
		writer_.String("QZ");
		writer_.Double(body.Rotation.Z);
		writer_.String("QW");
		writer_.Double(body.Rotation.W);

		writer_.String("Markers");
		writer_.StartArray();
		{
			for (const auto &marker : body.Markers)
			{
				if (marker.IsValid)
				{
					writer_.StartObject();
					{
						writer_.String("Id");
						writer_.Int(marker.Id);

						writer_.String("X");
						writer_.Double(marker.Position.X);
						writer_.String("Y");
						writer_.Double(marker.Position.Y);
						writer_.String("Z");
						writer_.Double(marker.Position.Z);
					}
					writer_.EndObject();
				}
			}

		}
		writer_.EndArray();
	}
	writer_.EndObject();
}

const char* JsonOptitrackMessage::Print()
{
	if (!is_sealed_)
	{
		writer_.EndArray();
		writer_.EndObject();
		is_sealed_ = true;
	}

	return buffer_.GetString();
}
