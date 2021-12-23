#!/bin/bash

PROGRAM='uitest.sh'
SHORTOPTS="h"
LONGOPTS="help,root:,config:,samples-profile:,samples-port:,samples-port-api:"
TEMP=$(getopt -o "$SHORTOPTS" -l "$LONGOPTS" -n "$PROGRAM" -- "$@")
if [ $? -ne 0 ]; then
        exit 1
fi
eval set -- "$TEMP"
unset TEMP

while true; do
    case "$1" in
        '-h' | '--help')
            cat <<EOF
Usage: $PROGRAM [options]

Options:
    -h, --help          Show this help.
    --root ROOT         Path to the repo root (default = \$DOTVVM_ROOT:-\$PWD).
    --config CONFIG     The build configuration (default = \$BUILD_CONFIGURATION:-Release).
    --samples-profile   (default = \$SAMPLES_PROFILE:-seleniumconfig.aspnetcorelatest.chrome.json).
    --samples-port      (default = 16019).
    --samples-port-api  (default = 5001).
EOF
            shift
            exit 0
        ;;
        '--root')
            DOTVVM_ROOT="$2"
            shift 2
            continue
        ;;
        '--config')
            BUILD_CONFIGURATION="$2"
            shift 2
            continue
        ;;
        '--samples-profile')
            SAMPLES_PROFILE="$2"
            shift 2
            continue
        ;;
        '--samples-port')
            SAMPLES_PORT="$2"
            shift 2
            continue
        ;;
        '--samples-port-api')
            SAMPLES_PORT_API="$2"
            shift 2
            continue
        ;;
        '--')
            shift
            break
        ;;
        *)
            echo >&2 "Option '$1' is not recognized."
            exit 1
        ;;
    esac
done

if [ -n "$1" ]; then
    echo >&2 "Argument '$1' is invalid."
    exit 1
fi

# ==================
# config var setting
# ==================

ROOT=${DOTVVM_ROOT:-$(pwd)}
# override DOTVVM_ROOT in case this is a local build
export DOTVVM_ROOT=$ROOT

CONFIGURATION="${BUILD_CONFIGURATION:-Release}"
DISPLAY="${DISPLAY:-":42"}"
SLN="$ROOT/ci/linux/Linux.sln"
export DISPLAY
TEST_RESULTS_DIR="$ROOT/artifacts/test"
SAMPLES_DIR="$ROOT/src/Samples/Tests/Tests"
SAMPLES_PROFILE="${SAMPLES_PROFILE:-seleniumconfig.aspnetcorelatest.chrome.json}"
SAMPLES_PORT="${SAMPLES_PORT:-16019}"
SAMPLES_PORT_API="${SAMPLES_PORT_API:-5001}"

echo "ROOT=$ROOT"
echo "SLN=$SLN"
echo "CONFIGURATION=$CONFIGURATION"
echo "DISPLAY=$DISPLAY"
echo "TEST_RESULTS_DIR=$TEST_RESULTS_DIR"
echo "SAMPLES_DIR=$SAMPLES_DIR"
echo "SAMPLES_PROFILE=$SAMPLES_PROFILE"
echo "SAMPLES_PORT=$SAMPLES_PORT"
echo "SAMPLES_PORT_API=$SAMPLES_PORT_API"

function start_group {
    echo "::group::$1"
}

function end_group {
    echo "::endgroup::"
}

function run_named_command {
    NAME=$1
    shift

    start_group $NAME
    echo "running '$@'"
    eval "$@"
    end_group
}

function ensure_named_command {
    NAME=$1
    run_named_command "$@"
    if [ $? -ne 0 ]; then
        echo >&2 "$NAME failed"
        exit 1
    fi
}

function clean_uitest {
    start_group "Kill processes"

    killall Xvfb dotnet chromium chromedriver 2>/dev/null
    rm /tmp/.X*-lock 2>/dev/null
    ps

    end_group
}

function start_samples {
    PROJECT=$1
    PORT=$2
    PID_VAR=$3
    dotnet run --project "$ROOT/${PROJECT}" \
        --no-restore \
        --configuration "$CONFIGURATION" \
        --urls "http://localhost:${PORT}/" >/dev/null &

    PID=$1
    eval "$PID_VAR=$PID"
    ps -p $PID >/dev/null
    if [ $? -ne 0 ]; then
        echo >&2 "The ${PROJECT} sample project failed to start."
        exit 1
    fi
    echo >&2 "The ${PROJECT} sample project is starting with PID=${SAMPLES_API_PID}."
    HTTP_CODE=$(curl localhost:5000 -s -o /dev/null -m 60 -w "%{http_code}")
    if [ $HTTP_CODE -ne 200 ]; then
        echo >&2 "Failed to start the ${PROJECT} sample project. Got a ${HTTP_CODE}."
        exit 1
    fi
}

# seleniumconfig.json needs to be copied before the build of the sln
PROFILE_PATH="$SAMPLES_DIR/Profiles/$SAMPLES_PROFILE"

if [ ! -f "$PROFILE_PATH" ]; then
    echo >&2 "Profile '$PROFILE_PATH' doesn't exist."
    exit 1
fi
cp -f "$PROFILE_PATH" "$SAMPLES_DIR/seleniumconfig.json"

clean_uitest

start_group "Start Xvfb"
{

    Xvfb $DISPLAY -screen 0 1920x1080x16 &
    XVFB_PID=$!
    if [ $? -ne 0 ]; then
        echo >&2 "Xvfb failed to start."
        exit 1
    fi
    echo >&2 "Xvfb running with PID=${XVFB_PID}."
}
end_group

start_group "Start samples"
{
    start_samples "src/Samples/Api.AspNetCoreLatest" "${SAMPLES_PORT_API}" "SAMPLES_API_PID"
    start_samples "src/Samples/AspNetCoreLatest" "${SAMPLES_PORT}" "SAMPLES_PID"
}
end_group

ps

start_group "Run UI tests"
{
    dotnet test "$SAMPLES_DIR" \
        --filter Category!=owin-only \
        --no-restore \
        --configuration $CONFIGURATION \
        --logger 'trx;LogFileName=ui-test-results.trx' \
        --results-directory "$TEST_RESULTS_DIR"
}
end_group

kill $XVFB_PID $SAMPLES_PID $SAMPLES_API_PID 2>/dev/null
clean_uitest
