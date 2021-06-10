#/bin/bash

# ================
# argument parsing
# ================

LONGOPTS='no-npm-build,no-sln-restore,no-sln-build,no-unit-tests,no-ui-tests'
TEMP=$(getopt --long '' -n 'DotVVM Linux CI' -- "$@")
if [ $? -ne 0 ]; then
        echo 'getopt failed' >&2
        exit 1
fi
eval set -- "$TEMP"
unset TEMP

NPM_BUILD=1
SLN_RESTORE=1
SLN_BUILD=1
UNIT_TESTS=1
UI_TESTS=1

while true; do
    case "$1" in
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
        '--no-ui-tests')
            UI_TESTS=0
            shift
            continue
        ;;
        *)
            echo 'a parsing error occured' >&2
            exit 1
        ;;
    esac
done

# ==================
# config var setting
# ==================

ROOT=${DOTVVM_ROOT:-$(pwd)}
# override DOTVVM_ROOT in case this is a local build
export DOTVVM_ROOT=$ROOT

TEST_RESULTS_DIR=$ROOT/artifacts/test
CONFIGURATION=${BUILD_CONFIGURATION:-Release}
DISPLAY=${DISPLAY:-":42"}
echo <<EOF
ROOT=$ROOT
TEST_RESULTS_DIR=$TEST_RESULTS_DIR
CONFIGURATION=$CONFIGURATION
DISPLAY=$DISPLAYT
EOF

# ================
# helper functions
# ================

function print_header {
    echo <<EOF
--------------------------------
$1
--------------------------------
EOF
}

function ensure_named_command {
    NAME=$1
    shift

    print_header $NAME
    eval $@
    if [ $? -ne 0 ]; then
        echo >&2 "$NAME failed"
        exit 1
    fi
}

# =============================
# actual continuous integration
# =============================

if [ $NPM_BUILD -eq 0 ]; then
    ensure_named_command "npm build" \
        cd $ROOT/src/DotVVM.Framework \
            && npm ci --cache ${ROOT}/.npm --prefer-offline \
            && npm run build
fi

if [ $SLN_BUILD -eq 0 ]; then
    ensure_named_command "sln build" \
        cd $ROOT \
            && dotnet restore $ROOT/ci/linux/Linux.sln --packages $ROOT/.nuget\
            && dotnet build $ROOT/ci/linux/Linux.sln \
                --no-restore \
                --configuration $CONFIGURATION
                -p:SourceLinkCreate=true
fi

fi [ $UNIT_TESTS -eq 0 ]; then
    ensure_named_command "unit tests" \
        dotnet test src/DotVVM.Framework.Tests \
            --no-build \
            --configuration $CONFIGURATION \
            --logger trx \
            --results-directory $TEST_RESULTS_DIR \
            --collect "Code Coverage"
fi

fi [ $UI_TESTS -eq 0 ]; then
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

    ensure_named_command "UI tests" \
        dotnet test src/DotVVM.Samples.Tests \
            --no-build \
            --configuration $CONFIGURATION \
            --logger trx \
            --results-directory $TEST_RESULTS_DIR

    kill $XVFB_PID $SAMPLES_PID 2>/dev/null
fi
