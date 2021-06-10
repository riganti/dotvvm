#/bin/bash

ROOT=${DOTVVM_ROOT:-$(pwd)}
# override DOTVVM_ROOT in case this is a local build
export DOTVVM_ROOT=$ROOT

CONFIGURATION=${BUILD_CONFIGURATION:-Release}
TEST_RESULTS_DIR=$ROOT/artifacts/test
DISPLAY=${DISPLAY:-":42"}
echo "ROOT=$ROOT"
echo "CONFIGURATION=$CONFIGURATION"

echo "--------------------------------"
echo "npm build"
echo "--------------------------------"
cd $ROOT/src/DotVVM.Framework \
    && npm ci --cache ${ROOT}/.npm --prefer-offline \
    && npm run build
if [ $? -ne 0 ]; then
    echo >&2 "npm build failed"
    exit 1
fi

echo "--------------------------------"
echo "sln build"
echo "--------------------------------"
cd $ROOT \
    && dotnet restore $ROOT/ci/linux/Linux.sln --packages $ROOT/.nuget\
    && dotnet build $ROOT/ci/linux/Linux.sln --no-restore --configuration $CONFIGURATION
if [ $? -ne 0 ]; then
    echo >&2 "dotnet build failed"
    exit 1
fi

echo "--------------------------------"
echo "unit tests"
echo "--------------------------------"
dotnet test src/DotVVM.Framework.Tests \
    --no-build \
    --configuration $CONFIGURATION \
    --logger trx \
    --results-directory $TEST_RESULTS_DIR \
    --collect "Code Coverage"

echo "--------------------------------"
echo "UI tests"
echo "--------------------------------"
killall Xvfb dotnet 2>/dev/null
rm /tmp/.X*-lock

Xvfb $DISPLAY -screen 0 800x600x16 &
XVFB_PID=$!

dotnet run --project src/DotVVM.Samples.BasicSamples.Api.AspNetCoreLatest \
    --no-build \
    --configuration $CONFIGURATION \
    --urls http://localhost:5001/ >/dev/null &

dotnet run --project src/DotVVM.Samples.BasicSamples.AspNetCoreLatest \
    --no-build \
    --configuration $CONFIGURATION \
    --urls http://localhost:16018/ >/dev/null &
SAMPLES_PID=$!

dotnet test src/DotVVM.Samples.Tests \
    --no-build \
    --configuration $CONFIGURATION \
    --logger trx \
    --results-directory $TEST_RESULTS_DIR

kill $XVFB_PID $SAMPLES_PID 2>/dev/null
