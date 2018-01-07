#!/bin/bash
cd /pipeline/source/app/publish
echo "Starting Order Command gRPC Service"

dotnet PartialFoods.Services.OrderCommandServer.dll