#pragma once

namespace Optitrack
{
	class Input
	{
	public:
		Input() {}
		virtual ~Input() {}

		virtual void Start() = 0;
		virtual void Stop() = 0;
	};
}
