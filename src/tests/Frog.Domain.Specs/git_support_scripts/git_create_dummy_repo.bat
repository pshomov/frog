cd %1
if EXIST %2 RMDIR /S/Q %2
mkdir %2
cd %2
cmd.exe /c git init .
echo line 1 > test.txt
cmd.exe /c git add test.txt
cmd.exe /c git commit -m "commit number one"