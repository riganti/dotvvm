call npm install
call npm install karma-cli -g
copy ..\DotVVM.Framework\Resources\Scripts\*.js .\*.js /y
karma start karma.ci.js