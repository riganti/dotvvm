#!/bin/sh

# when karma is not found, install is globally "npm install karma-cli -g"

npm install
cp ../DotVVM.Framework/Resources/Scripts/*.js .
cp ../DotVVM.Framework/Resources/Scripts/Globalize/globalize.js .
export CHROME_BIN="$(which chromium)"
karma start karma.ci.js
