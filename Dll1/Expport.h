#pragma once
#include "pch.h"

extern "C" HBITMAP __declspec(dllexport) Capture();
int Detect_MirrorDriver(std::vector<DISPLAY_DEVICE>& devices, std::map<int, DEVMODE>& settings);

