#pragma once
#include "pch.h"
#include "RDPCapture.h"
RDPCapture::RDPCapture() {};
RDPCapture::RDPCapture(DISPLAY_DEVICE dev, DEVMODE dm)
	{
		myDev = dev;
		myDM = dm;
		oldDM = dm;
		m_driverDC = NULL;
	}
RDPCapture::~RDPCapture()
	{
		SelectObject(m_cdc, Old_bitmap);
		DeleteObject(m_Bitmap);
		m_cdc.DeleteDC();
		if (m_driverDC != NULL) DeleteDC(m_driverDC);
		oldDM.dmDeviceName[0] = 0;
		ChangeDisplaySettingsEx(myDev.DeviceName, &oldDM, 0, 0, 0);
	}
 bool RDPCapture::Init(int x0, int y0, int width, int height)
	{
		mx0 = x0;
		my0 = y0;
		mw = (width + 3) & 0xFFFC;
		mh = height;

		DEVMODE dm;
		dm = myDM;
		WORD drvExtraSaved = dm.dmDriverExtra;
		memset(&dm, 0, sizeof(DEVMODE));
		dm.dmSize = sizeof(DEVMODE);
		dm.dmDriverExtra = drvExtraSaved;
		dm.dmPelsWidth = 2048;
		dm.dmPelsHeight = 1280;
		dm.dmBitsPerPel = 24;
		dm.dmPosition.x = 0;
		dm.dmPosition.y = 0;
		dm.dmDeviceName[0] = '\0';
		dm.dmFields = DM_BITSPERPEL | DM_PELSWIDTH |
			DM_PELSHEIGHT | DM_POSITION;
		if (ChangeDisplaySettingsEx(myDev.DeviceName, &dm, 0, CDS_UPDATEREGISTRY, 0))
		{
			ChangeDisplaySettingsEx(myDev.DeviceName, &dm, 0, 0, 0);
		}
		//------------------------------------------------
		ZeroMemory(&m_BmpInfo, sizeof(BITMAPINFO));
		m_BmpInfo.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
		m_BmpInfo.bmiHeader.biBitCount = 24;
		m_BmpInfo.bmiHeader.biCompression = BI_RGB;
		m_BmpInfo.bmiHeader.biPlanes = 1;
		m_BmpInfo.bmiHeader.biWidth = mw;
		m_BmpInfo.bmiHeader.biHeight = -mh;
		m_BmpInfo.bmiHeader.biSizeImage = mw * mh * 3;

		HDC Top = ::GetDC(GetDesktopWindow());
		int x = ::GetDeviceCaps(Top, HORZRES);       // 宽  
		int y = ::GetDeviceCaps(Top, VERTRES);
		m_cdc.CreateCompatibleDC(NULL);//兼容设备上下文环境
		m_Bitmap = CreateCompatibleBitmap(Top, x, y);//Bitmap,画布
		Old_bitmap = (HBITMAP)SelectObject(m_cdc, m_Bitmap);//画布与设备上下文环境关联
		::ReleaseDC(GetDesktopWindow(), Top);
		m_driverDC = CreateDC(myDev.DeviceName, 0, 0, 0);
		return true;
	};
 bool RDPCapture:: GetData(unsigned char* buf)
	{
		BitBlt(m_cdc, 0, 0, mw, mh, m_driverDC, mx0, my0, SRCCOPY | CAPTUREBLT);
		GetDIBits(m_cdc, m_Bitmap, 0, mh, buf, &m_BmpInfo, DIB_RGB_COLORS);
		//SaveBitmapToFile(m_Bitmap, L"test_temp.jpg");
		return true;
	};

	void  RDPCapture::SaveBitmapToFile(HBITMAP hBitmap, LPCWCHAR szfilename)
	{
		HDC hdc;			//设备描述表
		int ibits;
		WORD wbitcount;     //当前显示分辨率下每个像素所占字节数

		//位图中每个像素所占字节数，定义调色板大小，位图中像素字节大小，位图文件大小 ，写入文件字节数
		DWORD dwpalettesize = 0, dwbmbitssize, dwdibsize, dwwritten;

		BITMAP bitmap;				//位图属性结构
		BITMAPFILEHEADER bmfhdr;	//位图文件头结构
		BITMAPINFOHEADER bi;		//位图信息头结构
		LPBITMAPINFOHEADER lpbi;	//指向位图信息头结构

		//定义文件，分配内存句柄，调色板句柄
		HANDLE fh, hdib, hpal, holdpal = NULL;

		//计算位图文件每个像素所占字节数
		hdc = CreateDC(L"display", NULL, NULL, NULL);
		ibits = GetDeviceCaps(hdc, BITSPIXEL) * GetDeviceCaps(hdc, PLANES);
		DeleteDC(hdc);

		if (ibits <= 1)
			wbitcount = 1;
		else if (ibits <= 4)
			wbitcount = 4;
		else if (ibits <= 8)
			wbitcount = 8;
		else if (ibits <= 16)
			wbitcount = 16;
		else if (ibits <= 24)
			wbitcount = 24;
		else
			wbitcount = 32;

		//计算调色板大小
		if (wbitcount <= 8)
			dwpalettesize = (1 << wbitcount) * sizeof(RGBQUAD);

		//设置位图信息头结构
		GetObject(hBitmap, sizeof(BITMAP), (LPSTR)& bitmap);
		bi.biSize = sizeof(BITMAPINFOHEADER);
		bi.biWidth = bitmap.bmWidth;
		bi.biHeight = bitmap.bmHeight;
		bi.biPlanes = 1;
		bi.biBitCount = wbitcount;
		bi.biCompression = BI_RGB;
		bi.biSizeImage = 0;
		bi.biXPelsPerMeter = 0;
		bi.biYPelsPerMeter = 0;
		bi.biClrUsed = 0;
		bi.biClrImportant = 0;

		dwbmbitssize = ((bitmap.bmWidth * wbitcount + 31) / 32) * 4 * bitmap.bmHeight;
		//为位图内容分配内存
		hdib = GlobalAlloc(GHND, dwbmbitssize + dwpalettesize + sizeof(BITMAPINFOHEADER));
		lpbi = (LPBITMAPINFOHEADER)GlobalLock(hdib);
		*lpbi = bi;

		// 处理调色板 
		hpal = GetStockObject(DEFAULT_PALETTE);
		if (hpal)
		{
			hdc = ::GetDC(NULL);
			holdpal = SelectPalette(hdc, (HPALETTE)hpal, false);
			RealizePalette(hdc);
		}

		// 获取该调色板下新的像素值
		GetDIBits(hdc, hBitmap, 0, (UINT)bitmap.bmHeight, (LPSTR)lpbi + sizeof(BITMAPINFOHEADER) + dwpalettesize, (BITMAPINFO*)lpbi, DIB_RGB_COLORS);

		//恢复调色板 
		if (holdpal)
		{
			SelectPalette(hdc, (HPALETTE)holdpal, true);
			RealizePalette(hdc);
			::ReleaseDC(NULL, hdc);
		}

		//创建位图文件 
		fh = CreateFile(szfilename, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN, NULL);
		if (fh == INVALID_HANDLE_VALUE)
			return;

		// 设置位图文件头
		bmfhdr.bfType = 0x4d42; // "bm"
		dwdibsize = sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER) +
			dwpalettesize + dwbmbitssize;
		bmfhdr.bfSize = dwdibsize;
		bmfhdr.bfReserved1 = 0;
		bmfhdr.bfReserved2 = 0;
		bmfhdr.bfOffBits = (DWORD)sizeof(BITMAPFILEHEADER) +
			(DWORD)sizeof(BITMAPINFOHEADER) + dwpalettesize;

		//写入位图文件头
		WriteFile(fh, (LPSTR)& bmfhdr, sizeof(BITMAPFILEHEADER), &dwwritten, NULL);

		//写入位图文件其余内容
		WriteFile(fh, (LPSTR)lpbi, dwdibsize, &dwwritten, NULL);
		//清除 
		GlobalUnlock(hdib);
		GlobalFree(hdib);
		CloseHandle(fh);
	};