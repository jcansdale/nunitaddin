@echo off
@echo This build environment requires .NET Framework 3.5
@echo To build use 'msbuild NUnitAddIn.proj'

set path=%path%;%SystemRoot%\Microsoft.NET\Framework\v3.5
set path=%path%;%cd%\tools\WiX

