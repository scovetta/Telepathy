using namespace System::Reflection;
using namespace System::Runtime::CompilerServices;

[assembly: AssemblyVersion("5.0.0.0")];

[assembly: AssemblyCompany("Microsoft Corp.")];
[assembly: AssemblyProduct("Microsoft CoReXT")];
[assembly: AssemblyCopyright("2006")];



#if ENABLE_CODESIGN
#if !(BUILD_NO_GLOBAL_STRONG_NAME)
#if ENABLE_PRS_DELAYSIGN
[assembly: AssemblyDelaySign(true)];
[assembly: AssemblyKeyFile("")];
#else
[assembly: AssemblyKeyFile("")];
[assembly: AssemblyKeyName("")];
[assembly: AssemblyDelaySign(false)];
#endif
#endif
#endif
