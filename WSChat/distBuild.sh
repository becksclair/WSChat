#!/bin/bash

dotnet publish -c Release -r 'osx.11.0-x64' --self-contained true -p:PublishSingleFile=true
dotnet publish -c Release -r 'win10-x64' --self-contained true -p:PublishSingleFile=true
