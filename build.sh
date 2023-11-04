#!/bin/bash

unameOut="$(uname -a)"

if [ -z $machine ]; then
  case "${unameOut}" in
      Linux*)     machine=linux;;
      Darwin*)    machine=osx;;
      CYGWIN*)    machine=win;;
      MINGW*)     machine=win;;
      MSYS_NT*)   machine=win;;
      *)          machine=""
  esac
fi

if [ -z $arch ]; then
  case "${unameOut}" in
    *x86_64*)  arch=x64;;
    *x64*)     arch=x64;;
    *i386*)    arch=x86;;
    *i686*)    arch=x86;;
    *aarch64*) arch=arm64;;
	*arm64*)   arch=arm64;;
    *arm*)     arch=arm;;
    *)         arch=""
  esac
fi

if [ -z $machine ]; then
  echo "Unknown Platform"
  return 1
fi

if [ -z $arch ]; then
  echo "Unknown Architecture"
  return 1
fi

rid="$machine-$arch"
echo "RID: $rid"

dotnet publish --self-contained --framework net8.0 --runtime "$rid" --configuration "Release" --output "Build" Hazelnut.Wol.sln

if [ "$1" = "install" ]; then
  sudo cp Build/hznwol /usr/local/bin/hznwol
  sudo chmod +x /usr/local/bin/hznwol
fi
