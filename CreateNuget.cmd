echo off
set output=D:\Projects\packages
set bld=%~dp0
set nugetPath=%bld%\packages\NuGet.CommandLine.1.6.0\tools\nuget.exe

echo Path: %bld%
echo nuget: %nugetPath%
echo p2 %2
echo p3 %3
"%nugetPath%" pack "%bld%\PostsharpAspects\PostsharpAspects.nuspec" -Version %1 -OutputDirectory "%output%" -symbols -Properties Configuration=Release