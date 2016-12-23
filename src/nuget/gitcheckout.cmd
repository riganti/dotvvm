rem TFS leaves the source folder in detached state, so the git commit wouldn't work correctly
rem Argument 1 is branch name

git checkout %1
git pull origin