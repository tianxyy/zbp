#include "pch.h"
#include "Expport.h"
#include "RDPCapture.h"


extern "C" HBITMAP __declspec(dllexport) Capture() {


	unsigned char* data = NULL;
	Singleton<RDPCapture>::instance().GetData(data);
	return Singleton<RDPCapture>::instance().m_Bitmap;
	
}