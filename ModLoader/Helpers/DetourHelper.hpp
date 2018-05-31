#pragma once
#include "..\stdafx.h"

struct DetourInfo64 {
   DWORD64 origAddress;
   BYTE    origBytes[5];
   BYTE    detourBytes[5];

   void restore() {
      DWORD oldProtect;
      VirtualProtect((LPVOID)origAddress, 5, PAGE_EXECUTE_READWRITE, &oldProtect);
      memcpy_s((LPVOID)origAddress, 5, (LPVOID)origBytes, 5);
      VirtualProtect((LPVOID)origAddress, 5, oldProtect, &oldProtect);
   }
   void detour() {
      DWORD oldProtect;
      VirtualProtect((LPVOID)origAddress, 5, PAGE_EXECUTE_READWRITE, &oldProtect);
      memcpy_s((LPVOID)origAddress, 5, (LPVOID)detourBytes, 5);
      VirtualProtect((LPVOID)origAddress, 5, oldProtect, &oldProtect);
   }
};

class DetourHelper64 {
   DetourHelper64(const DetourHelper64&) = delete;

   void writeJMP(DWORD64& from, DWORD64& to) {
      DWORD relativeAddress = (DWORD)(to - from - 0x5);
      DWORD oldProtect;

      VirtualProtect((LPVOID)from, 5, PAGE_EXECUTE_READWRITE, &oldProtect);
      *(BYTE*)(from) = 0xE9;
      *(DWORD*)(from + 0x1) = relativeAddress;
      VirtualProtect((LPVOID)from, 5, oldProtect, &oldProtect);
   }

public:
   std::map<std::string, DetourInfo64> detouredFunctions;

   DetourHelper64() = default;
   ~DetourHelper64()
   {
      for (auto& dInfo : detouredFunctions)
         dInfo.second.restore();
      detouredFunctions.clear();
   }

   void detour(const std::string& functionDefinition, DWORD64 functionAddress, DWORD64 newFunctionAddress) {
      DetourInfo64 detourInfo = { 0 };
      detourInfo.origAddress = functionAddress;

      memcpy_s((LPVOID)detourInfo.origBytes, 5, (LPVOID)functionAddress, 5);
      writeJMP(functionAddress, newFunctionAddress);
      memcpy_s((LPVOID)detourInfo.detourBytes, 5, (LPVOID)functionAddress, 5);

      detouredFunctions.insert(std::make_pair(functionDefinition, detourInfo));
   }
};