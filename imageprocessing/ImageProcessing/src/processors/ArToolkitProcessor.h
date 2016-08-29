#pragma once

#include <AR/ar.h>
#include "processors/Processor.h"
#include "processors/ARMarkerSquare.h"

namespace ImageProcessing
{
	typedef struct {
		ARdouble T[16]; // Position and orientation, column-major order. (position(x,y,z) = {T[12], T[13], T[14]}
	} ARPose;

	class ArToolkitProcessor : public Processor
	{
	private:
		bool is_initialized_;
		// Markers
		// Marker detection
		ARMarkerSquare *markersSquare = nullptr;
		int markersSquareCount = 0;
		ARHandle		*gARHandleL = nullptr;
		ARHandle		*gARHandleR = nullptr;
		long			 gCallCountMarkerDetect = 0;
		ARPattHandle	*gARPattHandle = nullptr;
		int           gARPattDetectionMode;

		// Transformation matrix retrieval
		AR3DHandle	*gAR3DHandleL = nullptr;
		AR3DHandle	*gAR3DHandleR = nullptr;
		AR3DStereoHandle	*gAR3DStereoHandle = nullptr;
		ARdouble      transL2R[3][4];
		ARdouble      transR2L[3][4];;

		// Drawing.
		ARParamLT *gCparamLTL = nullptr;
		ARParamLT *gCparamLTR = nullptr;


	public:
		virtual ~ArToolkitProcessor();
		virtual std::shared_ptr<const FrameData> Process(const std::shared_ptr<const FrameData> &frame) override;

	private:
		void Initialize(int sizeX, int sizeY);
		bool SetupCamera(std::string filename, int sizeX, int sizeY, ARParamLT **cparamLT_p);
		void Cleanup();
	};
}
