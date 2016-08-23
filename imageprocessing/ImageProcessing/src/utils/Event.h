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
			: _mutex(),
			  _handlers()
		{

		}


		void Call(EventType arg)
		{
			std::unique_lock<std::mutex> lock(_mutex);
			for (auto handler : _handlers)
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
			std::unique_lock<std::mutex> lock(_mutex);
			_handlers.push_back(handler);
			return *this;
		}


		Event& operator -= (EventHandler handler)
		{
			std::unique_lock<std::mutex> lock(_mutex);
			_handlers.erase(std::remove(_handlers.begin(), _handlers.end(),  handler), _handlers.end());
		}


	private:
		std::vector<EventHandler> _handlers;
		std::mutex _mutex;
	};
}
