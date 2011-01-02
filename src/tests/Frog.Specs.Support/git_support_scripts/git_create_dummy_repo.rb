#!/usr/bin/env ruby
Dir.chdir ARGV[0]
Dir.delete ARGV[1] if File.exists? ARGV[1]
Dir.mkdir ARGV[1]
Dir.chdir ARGV[1]
system 'git init .'
exit $? if $? != 0
open('test.txt', 'w') {|fd| fd.puts "line 1"}
system 'git add test.txt'
exit $? if $? != 0
system 'git commit -m "commit number one"'
exit $? if $? != 0

#
#%~d1
#cd %1
#if EXIST %2 RMDIR /S/Q %2
#mkdir %2
#cd %2
#cmd.exe /c git init .
#echo line 1 > test.txt
#cmd.exe /c git add test.txt
#cmd.exe /c git commit -m "commit number one"