call npm install
call npm install karma-cli -g
rem copy ..\DotVVM.Framework\Resources\Scripts\*.js .\*.js /y
rem copy ..\DotVVM.Framework\Resources\Scripts\Globalize\globalize.js .\globalize.js /y
rem karma start karma.ci.js