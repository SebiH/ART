#pragma once

#include <cstdlib>
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

		int id;
		Type type;
		std::unique_ptr<unsigned char[]> data;
		cv::Mat img;

		ProcessingOutput()
		{
			// TODO: count up ids or something?
			id = rand();
		}

		// hide copy constructors due to unique_ptr member
		ProcessingOutput(ProcessingOutput const &) = delete;
		ProcessingOutput &operator=(ProcessingOutput const &) = delete;

		// .. but define a move operator instead
		ProcessingOutput(ProcessingOutput &&other)
			: id(other.id),
			  type(other.type),
			  img(other.img),
			  data(std::move(other.data))
		{}

		ProcessingOutput &operator=(ProcessingOutput &&other)
		{
			if (this != &other)
			{
				id = other.id;
				type = other.type;
				img = other.img;
				data = std::move(other.data);
			}

			return *this;
		}
	};
}
