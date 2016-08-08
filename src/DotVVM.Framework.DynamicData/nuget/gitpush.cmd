rem Commit changed package version
rem Argument 1 is version number
rem Argument 2 is branch name

git config --global user.email "tfs@riganti.cz"
git config --global user.name "TFS"
git commit -am "NuGet package version %1"
git rebase HEAD %2
git push https://rigantiteamcity:2tt1JW0QkJaj5n@github.com/riganti/dotvvm-dynamicdata.git %2