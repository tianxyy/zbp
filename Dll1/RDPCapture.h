#pragma once
#include "pch.h"
class RDPCapture
{
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
public:
	HBITMAP	m_Bitmap;
	HBITMAP	Old_bitmap;
	RDPCapture();
	RDPCapture(DISPLAY_DEVICE dev, DEVMODE dm);
	~RDPCapture();
	virtual bool Init(int x0, int y0, int width, int height);	
	virtual bool GetData(unsigned char* buf);
	void SaveBitmapToFile(HBITMAP hBitmap, LPCWCHAR szfilename);
};

