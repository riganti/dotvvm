#/bin/bash

# ================
# argument parsing
# ================

PROGRAM='run.sh'
SHORTOPTS="h"
LONGOPTS="help,\
    root:,config:,samples-profile:,samples-port:,samples-port-api:,\
    all,clean,npm-build,sln-restore,sln-build,unit-tests,js-tests,ui-tests,\
    no-all,no-clean,no-npm-build,no-sln-restore,no-sln-build,no-unit-tests,no-js-tests,no-ui-tests"
TEMP=$(getopt -o "$SHORTOPS" -l "$LONGOPTS" -n "$PROGRAM" -- "$@")
if [ $? -ne 0 ]; then
        exit 1
fi
eval set -- "$TEMP"
unset TEMP

CLEAN=0
NPM_BUILD=1
SLN_RESTORE=1
SLN_BUILD=1
UNIT_TESTS=1
JS_TESTS=1
UI_TESTS=1

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
    --[no-]all          Enable or disable all phases.
    --[no-]clean        Clean the with 'git clean' first (default = 0).
    --[no-]npm-build    Build the JS part of the Framework (default = 1).
    --[no-]sln-restore  Restore NuGet packages (default = 1).
    --[no-]sln-build    Build ~/ci/linux/Linux.sln (default = 1).
    --[no-]unit-tests   Run the Framework tests (default = 1).
    --[no-]js-tests     Run Framework's Jest tests (default = 1).
    --[no-]ui-tests     Run the AspNetCoreLatest tests (default = 1).
EOF
            shift
            exit 0
        ;;
        '--all'|'--no-all')
            [[ "$1" = --no-* ]]
            VALUE=$?
            CLEAN=$VALUE
            NPM_BUILD=$VALUE
            SLN_RESTORE=$VALUE
            SLN_BUILD=$VALUE
            UNIT_TESTS=$VALUE
            JS_TESTS=$VALUE
            UI_TESTS=$VALUE
            shift
            continue
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
            # handle all flags
            [[ "$1" = --no-* ]]
            VALUE=$?
            if [ $VALUE -eq 1 ]; then
                OPTION="${1#--}";
            else
                OPTION="${1#--no-}";
            fi

            OPTION=${OPTION^^}
            OPTION=${OPTION//-/_}
            eval IS_VALID=[ -n \$$OPTION ]
            if [ $IS_VALID -eq 0 ]; then
                # this flag doesn't exit
                echo >&2 "Option '$1' is not recognized."
                exit 1
            fi

            eval $OPTION=$VALUE
            shift
            continue;
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
TEST_RESULTS_DIR="$ROOT/artifacts/test"
SAMPLES_PROFILE="${SAMPLES_PROFILE:-seleniumconfig.aspnetcorelatest.chrome.json}"
SAMPLES_PORT="${SAMPLES_PORT:-16019}"
SAMPLES_PORT_API="${SAMPLES_PORT_API:-5001}"

tput sgr0
echo "ROOT=$ROOT"
echo "SLN=$SLN"
echo "CONFIGURATION=$CONFIGURATION"
echo "DISPLAY=$DISPLAY"
echo "TEST_RESULTS_DIR=$TEST_RESULTS_DIR"
echo "SAMPLES_PROFILE=$SAMPLES_PROFILE"
echo "SAMPLES_PORT=$SAMPLES_PORT"
echo "SAMPLES_PORT_API=$SAMPLES_PORT_API"

# ================
# helper functions
# ================

function print_header {
    tput sgr0
    echo "$(tput setaf 3)--------------------------------"
    echo "$(tput setaf 3 && tput smso)$@$(tput rmso)"
    echo "$(tput setaf 3)--------------------------------"
    tput sgr0
}

function run_named_command {
    NAME=$1
    shift

    print_header $NAME
    tput setaf 4 && printf "running '$@'\n" && tput sgr0
    eval $@
}

function ensure_named_command {
    NAME=$1
    run_named_command "$@"
    if [ $? -ne 0 ]; then
        echo >&2 "$NAME failed"
        exit 1
    fi
}

# =============================
# actual continuous integration
# =============================

if [ $CLEAN -eq 1 ]; then
    ensure_named_command "clean" "git clean -dfx"
fi

if [ $NPM_BUILD -eq 1 ]; then
    ensure_named_command "npm build" \
        "cd \"$ROOT/src/DotVVM.Framework\" \
            && npm ci --cache \"$ROOT/.npm\" --prefer-offline \
            && npm run build"
fi

if [ $SLN_RESTORE -eq 1 ]; then
    ensure_named_command "sln restore" \
        "cd $ROOT \
            && dotnet restore \"$SLN\" \
                --packages \"$ROOT/.nuget\" \
                -v:m"
fi

if [ $SLN_BUILD -eq 1 ]; then
    ensure_named_command "sln build" \
        "cd \"$ROOT\" \
            && dotnet build \"$SLN\" \
                --no-restore \
                --configuration $CONFIGURATION \
                -v:m \
                -p:SourceLinkCreate=true"
fi

if [ $UNIT_TESTS -eq 1 ]; then
    run_named_command "unit tests" \
        "dotnet test \"$ROOT/src/DotVVM.Framework.Tests\" \
            --no-build \
            --configuration $CONFIGURATION \
            --logger trx \
            --results-directory \"$TEST_RESULTS_DIR\" \
            --collect \"Code Coverage\""
fi

if [ $JS_TESTS -eq 1 ]; then
    run_named_command "js tests" \
        "cd \"$ROOT/src/DotVVM.Framework\" \
            && npx jest --ci --reporters=\"jest-junit\" \
            && cp ./junit.xml \"$TEST_RESULTS_DIR/js-test-results.xml\" \
            && cd \"$ROOT\""
fi

if [ $UI_TESTS -eq 1 ]; then
    killall Xvfb dotnet 2>/dev/null
    rm /tmp/.X*-lock

    Xvfb $DISPLAY -screen 0 800x600x16 &
    XVFB_PID=$!

    dotnet run --project "$ROOT/src/DotVVM.Samples.BasicSamples.Api.AspNetCoreLatest" \
        --no-build \
        --configuration "$CONFIGURATION" \
        --urls "http://localhost:${SAMPLES_PORT_API}/" >/dev/null &
    SAMPLES_API_PID=$!

    dotnet run --project "$ROOT/src/DotVVM.Samples.BasicSamples.AspNetCoreLatest" \
        --no-build \
        --configuration "$CONFIGURATION" \
        --urls "http://localhost:${SAMPLES_PORT}/" >/dev/null &
    SAMPLES_PID=$!

    SAMPLES_DIR="$ROOT/src/DotVVM.Samples.Tests"
    PROFILE_PATH="$SAMPLES_DIR/Profiles/$SELENIUM_CONFIG"

    if [ ! -f "$PROFILE_PATH" ]; then
        echo >&2 "Profile '$PROFILE_PATH' doesn't exist."
    fi
    cp -f "$PROFILE_PATH" "$SAMPLES_DIR/seleniumconfig.json"

    run_named_command "UI tests" \
        "dotnet test \"$SAMPLES_DIR\" \
            --no-build \
            --configuration $CONFIGURATION \
            --logger trx \
            --results-directory \"$TEST_RESULTS_DIR\""

    kill $XVFB_PID $SAMPLES_PID $SAMPLES_API_PID 2>/dev/null
fi
