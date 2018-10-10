call npm install
call npm install karma-cli -g
copy ..\DotVVM.Framework\Resources\Scripts\*.js .\*.js /y
copy ..\DotVVM.Framework\Resources\Scripts\Globalize\globalize.js .\globalize.js /y
karma start karma.ci.js