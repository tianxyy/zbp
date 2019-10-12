#pragma once
#include "pch.h"
#include "Singleton.h"
class RDPCapture
{
private :
	
	int mw;
	int mh;
	int mx0;
	int my0;
	DEVMODE myDM;
	DISPLAY_DEVICE myDev;
	HDC m_driverDC;
	CDC m_cdc;
	BITMAPINFO	m_BmpInfo;
	DEVMODE oldDM;
	
	HBITMAP	Old_bitmap;
public:
	friend class Singleton<RDPCapture>;
	virtual bool GetData(unsigned char* buf);
	HBITMAP	m_Bitmap;
	
protected:
	RDPCapture();
	RDPCapture(DISPLAY_DEVICE dev, DEVMODE dm);
	~RDPCapture();
	virtual bool Init();
	void SaveBitmapToFile(HBITMAP hBitmap, LPCWCHAR szfilename);
	int Detect_MirrorDriver(std::vector<DISPLAY_DEVICE>& devices, std::map<int, DEVMODE>& settings);

	

};

