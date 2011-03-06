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
