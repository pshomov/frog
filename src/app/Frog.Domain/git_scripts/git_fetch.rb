#!/usr/bin/env ruby

Dir.chdir ARGV[2]
system 'git init .'
exit $? if $? != 0
system "git remote add origin #{ARGV[0]}"
exit $? if $? != 0
system 'git remote update'
exit $? if $? != 0
system 'git checkout -b master --track origin/master'
exit $? if $? != 0
system "git checkout #{ARGV[1]}"
exit $?
