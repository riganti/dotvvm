#/bin/bash

# ================
# argument parsing
# ================

PROGRAM='DotVVM Linux CI'
SHORTOPTS='-h'
LONGOPTS='help,no-all,no-npm-build,no-sln-restore,no-sln-build,no-unit-tests,no-js-tests,no-ui-tests'
TEMP=$(getopt -o "$SHORTOPS" -l "$LONGOPTS" -n "$PROGRAM" -- "$@")
if [ $? -ne 0 ]; then
        exit 1
fi
eval set -- "$TEMP"
unset TEMP

NPM_BUILD=1
SLN_RESTORE=1
SLN_BUILD=1
UNIT_TESTS=1
JS_TESTS=1
UI_TESTS=1

while true; do
    case "$1" in
        '-h' | '--help')
            echo <<EOF
Usage: $0 [options]
Options:
    -h, --help          Show this help.
    --no-npm-build      Don't build the JS part of the Framework.
    --no-sln-restore    Don't restore NuGet packages.
    --no-sln-build      Don't build ~/ci/linux/Linux.sln.
    --no-unit-tests     Don't run the Framework tests.
    --no-js-tests       Don't run Framework's Jest tests.
    --no-ui-tests       Don't run the AspNetCoreLatest tests.
EOF
            shift
            continue
        ;;
        '--no-npm-build')
            NPM_BUILD=0
            shift
            continue
        ;;
        '--no-sln-restore')
            SLN_RESTORE=0
            shift
            continue
        ;;
        '--no-sln-build')
            SLN_BUILD=0
            shift
            continue
        ;;
        '--no-unit-tests')
            UNIT_TESTS=0
            shift
            continue
        ;;
        '--no-js-tests')
            JS_TESTS=0
            shift
            continue
        ;;
        '--no-ui-tests')
            UI_TESTS=0
            shift
            continue
        ;;
        '--no-all')
            NPM_BUILD=0
            SLN_RESTORE=0
            SLN_BUILD=0
            UNIT_TESTS=0
            JS_TESTS=0
            UI_TESTS=0
            shift
            continue
        ;;
        '--')
            shift
            break
        ;;
        *)
            echo >&2 "A parsing error occured at option '$1'."
            exit 1
        ;;
    esac
done

if [ -n "$1" ]; then
    echo >&2 "A parsing error occured at argument '$1'."
    exit 1
fi

# ==================
# config var setting
# ==================

ROOT=${DOTVVM_ROOT:-$(pwd)}
# override DOTVVM_ROOT in case this is a local build
export DOTVVM_ROOT=$ROOT

CONFIGURATION=${BUILD_CONFIGURATION:-Release}
DISPLAY=${DISPLAY:-":42"}
SLN=$ROOT/ci/linux/Linux.sln
TEST_RESULTS_DIR=$ROOT/artifacts/test

tput sgr0
echo "ROOT=$ROOT"
echo "SLN=$SLN"
echo "CONFIGURATION=$CONFIGURATION"
echo "DISPLAY=$DISPLAY"
echo "TEST_RESULTS_DIR=$TEST_RESULTS_DIR"

# ================
# helper functions
# ================

function print_header {
    tput sgr0 && tput setaf 3
    echo "--------------------------------"
    echo "$(tput smso)$@$(tput rmso)"
    echo "--------------------------------"
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
    run_named_command $@
    if [ $? -ne 0 ]; then
        echo >&2 "$NAME failed"
        exit 1
    fi
}

# =============================
# actual continuous integration
# =============================

if [ $NPM_BUILD -eq 1 ]; then
    ensure_named_command "npm build" \
        "cd $ROOT/src/DotVVM.Framework \
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

if [ $UI_TESTS -eq 1 ]; then
    killall Xvfb dotnet 2>/dev/null
    rm /tmp/.X*-lock

    Xvfb $DISPLAY -screen 0 800x600x16 &
    XVFB_PID=$!

    dotnet run --project "$ROOT/src/DotVVM.Samples.BasicSamples.Api.AspNetCoreLatest" \
        --no-build \
        --configuration "$CONFIGURATION" \
        --urls http://localhost:5001/ >/dev/null &

    dotnet run --project "$ROOT/src/DotVVM.Samples.BasicSamples.AspNetCoreLatest" \
        --no-build \
        --configuration "$CONFIGURATION" \
        --urls http://localhost:16018/ >/dev/null &
    SAMPLES_PID=$!

    run_named_command "UI tests" \
        "dotnet test \"$ROOT/src/DotVVM.Samples.Tests\" \
            --no-build \
            --configuration $CONFIGURATION \
            --logger trx \
            --results-directory \"$TEST_RESULTS_DIR\""

    kill $XVFB_PID $SAMPLES_PID 2>/dev/null
fi
