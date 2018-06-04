#pragma once
#include "..\stdafx.h"

enum class LogType {
   Default = 0,
   Info = 1,
   Warning = 2,
   Error = 3
};

class Logger {
   std::string logHeader;
   const char  logHeaderPre  = '[';
   const char  logHeaderPost = ']';
   std::string logWarningHeader;
   std::string logErrorHeader;
public:
   Logger(const std::string& _logHeader, const std::string _logWarningHeader = "WARNING!", const std::string _logErrorHeader = "ERROR!!"): 
      logHeader(_logHeader), logWarningHeader(_logWarningHeader), logErrorHeader(_logErrorHeader) {}

   void log(const std::string& logMessage, LogType logType = LogType::Default) {
      switch (logType)
      {
         case LogType::Default:
         case LogType::Info:
            std::cout << logHeaderPre << logHeader << logHeaderPost << ' ' << logMessage << std::endl;
            break;
         case LogType::Warning:
            std::cout << logWarningHeader << ' ' << logHeaderPre << logHeader << logHeaderPost << ' ' << logMessage << std::endl;
            break;
         case LogType::Error:
            std::cerr << logErrorHeader << ' ' << logHeaderPre << logHeader << logHeaderPost << ' ' << logMessage << std::endl;
            break;
      }
   }
};