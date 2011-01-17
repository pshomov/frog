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
