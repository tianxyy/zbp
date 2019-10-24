#include "pch.h"
#include "Expport.h"
#include "RDPCapture.h"


extern "C" HBITMAP __declspec(dllexport) Capture() {
	unsigned char* data = NULL;
	RDPCapture& rdp_capture = Singleton<RDPCapture>::instance();
	rdp_capture.GetData(data);
	return rdp_capture.m_Bitmap;	
}