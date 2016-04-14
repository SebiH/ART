#pragma once

#include <memory>
#include <utility>
#include <opencv2/core.hpp>

namespace ImageProcessing
{
	struct ProcessingOutput
	{
		enum class Type
		{
			left = 0,
			right = 1,
			combined =2
		};

		Type type;
		std::unique_ptr<unsigned char[]> data;
		cv::Mat img;


		ProcessingOutput() {}

		// hide copy constructors due to unique_ptr member
		ProcessingOutput(ProcessingOutput const &) = delete;
		ProcessingOutput &operator=(ProcessingOutput const &) = delete;

		// .. but define a move operator instead
		ProcessingOutput(ProcessingOutput &&other)
			: type(other.type),
			  img(other.img),
			  data(std::move(other.data))
		{}

		ProcessingOutput &operator=(ProcessingOutput &&other)
		{
			if (this != &other)
			{
				type = other.type;
				img = other.img;
				data = std::move(other.data);
			}

			return *this;
		}
	};
}
