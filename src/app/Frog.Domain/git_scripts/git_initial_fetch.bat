cd %1
if EXIST %2 RMDIR /S/Q %2
mkdir %2
cd %2
cmd.exe /c git init .

cmd.exe /c git remote add origin %3
cmd.exe /c git remote update
cmd.exe /c git checkout -b master --track origin/master