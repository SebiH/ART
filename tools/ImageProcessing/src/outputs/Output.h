#pragma once

#include <Windows.h> // slim read/write lock
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

	public:
		Output() : id_(UIDGenerator::Instance()->GetUID()), last_written_frameid(-1)
        {
            lock_ = new SRWLOCK();
            InitializeSRWLock(lock_);
        }
		virtual ~Output() { }


		UID Id() const { return id_; }

		virtual void RegisterResult(const std::shared_ptr<const FrameData> &result)
		{
            AcquireSRWLockExclusive(lock_);
            current_result_ = result;
            ReleaseSRWLockExclusive(lock_);
        }

		virtual void WriteResult()
		{
            AcquireSRWLockShared(lock_);

			if (current_result_ && current_result_->id > last_written_frameid)
			{
				Write(current_result_.get());
				last_written_frameid = current_result_->id;
			}

            ReleaseSRWLockShared(lock_);
		}

    protected:
        PSRWLOCK lock_;
        virtual void Write(const FrameData *result) noexcept = 0;
	};
}
