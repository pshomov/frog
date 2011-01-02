#!/usr/bin/env ruby

Dir.chdir ARGV[0]
Dir.delete ARGV[1] if File.exists? ARGV[1]
Dir.mkdir ARGV[1]
Dir.chdir ARGV[1]
system 'git init .'
exit $? if $? != 0
system "git remote add origin #{ARGV[2]}"
exit $? if $? != 0
system 'git remote update'
exit $? if $? != 0
system 'git checkout -b master --track origin/master'
exit $? if $? != 0

#cd %1
#if %errorlevel% neq 0 exit /b %errorlevel%
#
#if EXIST %2 RMDIR /S/Q %2
#if %errorlevel% neq 0 exit /b %errorlevel%
#
#mkdir %2
#if %errorlevel% neq 0 exit /b %errorlevel%
#
#cd %2
#if %errorlevel% neq 0 exit /b %errorlevel%
#
#cmd.exe /c git init .
#if %errorlevel% neq 0 exit /b %errorlevel%
#
#cmd.exe /c git remote add origin %3
#if %errorlevel% neq 0 exit /b %errorlevel%
#
#cmd.exe /c git remote update
#if %errorlevel% neq 0 exit /b %errorlevel%
#
#cmd.exe /c git checkout -b master --track origin/master
#if %errorlevel% neq 0 exit /b %errorlevel%