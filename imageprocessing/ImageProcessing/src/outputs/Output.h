#pragma once

#include <mutex>
#include "frames/FrameData.h"
#include "utils/UID.h"
#include "utils/UIDGenerator.h"

namespace ImageProcessing
{
	class Output
	{
	private:
		const UID id_;
		std::shared_ptr<const FrameData> current_result_;
		UID last_written_frameid;
		std::mutex result_mutex_;

	public:
		Output() : id_(UIDGenerator::Instance()->GetUID()), result_mutex_(), last_written_frameid(-1) { }
		virtual ~Output() { }


		UID Id() const { return id_; }

		virtual void RegisterResult(const std::shared_ptr<const FrameData> &result)
		{
			std::lock_guard<std::mutex> lock(result_mutex_);
			current_result_ = result;
		}

		virtual void WriteResult()
		{
			std::shared_ptr<const FrameData> local_result;

			{
				std::lock_guard<std::mutex> lock(result_mutex_);
				local_result = current_result_;
			}

			if (local_result && local_result->id > last_written_frameid)
			{
				Write(local_result.get());
				last_written_frameid = local_result->id;
			}
		}

	protected:
		virtual void Write(const FrameData *result) noexcept = 0;
	};
}
