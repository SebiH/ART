#pragma once

#include <algorithm>
#include <mutex>
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
			handlers_.erase(std::remove_if(handlers_.begin(), handlers_.end(), [handler](const EventHandler &v_handler) {
				return handler.target<void(EventType)>() == v_handler.target<void(EventType)>();
			}), handlers_.end());
			return *this;
		}


	private:
		std::vector<EventHandler> handlers_;
		std::mutex mutex_;
	};
}
