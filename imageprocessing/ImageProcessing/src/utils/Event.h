#pragma once

#include <mutex>
#include <functional>
#include <vector>

namespace ImageProcessing
{
	template<typename EventType>
	class Event
	{
	public:
		typedef std::function<void(EventType)> EventHandler;

	public:
		Event()
			: mutex_(),
			  handlers_()
		{

		}
		
		virtual ~Event() { }

		void Call(EventType arg)
		{
			std::unique_lock<std::mutex> lock(mutex_);
			for (auto handler : handlers_)
			{
				handler(arg);
			}
		}

		void operator ()(EventType arg)
		{
			Call(arg);
		}


		Event& operator += (EventHandler handler)
		{
			std::unique_lock<std::mutex> lock(mutex_);
			handlers_.push_back(handler);
			return *this;
		}


		Event& operator -= (EventHandler handler)
		{
			std::unique_lock<std::mutex> lock(mutex_);
			handlers_.erase(std::remove(handlers_.begin(), handlers_.end(),  handler), handlers_.end());
		}


	private:
		std::vector<EventHandler> handlers_;
		std::mutex mutex_;
	};
}
