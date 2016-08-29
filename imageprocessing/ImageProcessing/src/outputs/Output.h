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
			std::unique_lock<std::mutex> lock(result_mutex_);
			current_result_ = result;
		}

		virtual void WriteResult()
		{
			std::unique_lock<std::mutex> lock(result_mutex_);
			if (current_result_ && current_result_->id > last_written_frameid)
			{
				Write(current_result_.get());
				last_written_frameid = current_result_->id;
			}
		}

	protected:
		virtual void Write(const FrameData *result) noexcept = 0;
	};
}
