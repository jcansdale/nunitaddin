@echo off
@echo This build environment requires .NET Framework 2.0 RTM (v2.0.50727)
@echo To build use 'msbuild NUnitAddIn.proj'

set path=%path%;%SystemRoot%\Microsoft.NET\Framework\v2.0.50727
set path=%path%;%cd%\tools\WiX
