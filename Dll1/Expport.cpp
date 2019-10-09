#include "pch.h"
#include "Expport.h"
#include "RDPCapture.h"
int Detect_MirrorDriver(std::vector<DISPLAY_DEVICE>& devices, std::map<int, DEVMODE>& settings)
{
	CString all_mirror_divers[2] = {
					_T("RDP Encoder Mirror Driver"),//included in windows 7
					_T("LEDXXX Mirror Driver")//my own mirror driver, used in Windows XP
	};
	DISPLAY_DEVICE dd;
	ZeroMemory(&dd, sizeof(dd));
	dd.cb = sizeof(dd);
	int n = 0;
	while (EnumDisplayDevices(NULL, n, &dd, EDD_GET_DEVICE_INTERFACE_NAME))
	{
		n++;
		devices.push_back(dd);
	}
	for (int i = 0; i < (int)devices.size(); i++)
	{
		DEVMODE dm;
		ZeroMemory(&dm, sizeof(DEVMODE));
		dm.dmSize = sizeof(DEVMODE);
		dm.dmDriverExtra = 0;
		if (EnumDisplaySettingsEx(devices[i].DeviceName, ENUM_CURRENT_SETTINGS, &dm, EDS_ROTATEDMODE))
		{
			settings.insert(std::map<int, DEVMODE>::value_type(i, dm));
		}
	}
	for (int m = 0; m < 2; m++)
	{
		for (int i = 0; i < (int)devices.size(); i++)
		{
			CString drv(devices[i].DeviceString);
			if (drv == all_mirror_divers[m])
			{
				return m;
			}
		}
	}
	return -1;//can not use any mirror driver
};

extern "C" HBITMAP __declspec(dllexport) Capture() {

	RDPCapture* rdp_capture = NULL;
	std::vector<DISPLAY_DEVICE> devices;
	std::map<int, DEVMODE> settings;


	int res = Detect_MirrorDriver(devices, settings);
	if (res == 0)
	{
		rdp_capture = new RDPCapture(devices[0], settings[0]);
		rdp_capture->Init(0, 0, 1920, 1080);
		unsigned char* data = NULL;
		rdp_capture->GetData(data);
		
		return rdp_capture->m_Bitmap;
	}


	return NULL;
}