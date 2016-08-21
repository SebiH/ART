#pragma once

#include <AR/ar.h>

#include "IProcessingModule.h"
#include "ARMarkerSquare.h"

namespace ImageProcessing
{
	class ARToolkitModule : public IProcessingModule
	{
	private:
		bool isInitialized = false;

		// Markers
		ARMarkerSquare *markersSquare = nullptr;
		int markersSquareCount = 0;

		// Marker detection
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

		// TODO: refactor (quickly hacked together)
		bool newMarkerMatrix = false;
		double *markerMatrix;

	public:
		ARToolkitModule();
		~ARToolkitModule();
		virtual std::vector<ProcessingOutput> processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight, const ImageInfo &info) override;
		
		// TODO: refactor (quickly hacked together!)
		bool hasNewMarkerDetected();
		double* getNewMarkerMatrix() const;

	private:
		void initialize(int sizeX, int sizeY);
		bool setupCamera(std::string filename, int sizeX, int sizeY, ARParamLT **cparamLT_p);
		void cleanup();
	};

}
