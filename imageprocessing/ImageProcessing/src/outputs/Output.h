#pragma once

#include "utils/UID.h"
#include "utils/UIDGenerator.h"

namespace ImageProcessing
{
	class Output
	{
	private:
		const UID id_;

	public:
		Output() : id_(UIDGenerator::Instance()->GetUID()) { }
		virtual ~Output() { }

		UID Id() const { return id_; }
	};
}
