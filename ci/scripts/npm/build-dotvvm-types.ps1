param([Parameter(Mandatory=$true)] $version)

$dir = pwd
push-location ../../../src/Framework/Framework

npm run tsc-types
if (-not (test-path "$dir/dotvvm-types/types")) {
    mkdir "$dir/dotvvm-types/types"
}
copy ./obj/typescript-types/dotvvm.d.ts "$dir/dotvvm-types/types/index.d.ts"

pop-location

push-location dotvvm-types

npm version $version --no-git-tag-version

pop-location