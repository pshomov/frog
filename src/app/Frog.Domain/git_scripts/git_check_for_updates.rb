#!/usr/bin/env ruby

def exitcode status
  puts "exit code with #{status}"
  if status.exitstatus > 255
    d = status.exitstatus - 255
    puts "leaving"
    d
  else
    puts "leaving"
    status.exitstatus
  end
end

Dir.chdir "#{ARGV[0]}/#{ARGV[1]}"
system 'git remote update'
exit $? if $? != 0
system "git log HEAD..origin/master --exit-code"
ec = exitcode($?)
if ec == 1
  puts "merging ..."
  system 'git merge origin/master'
  puts "merging exit code is #{exitcode($?)}"
#  exit 150 if exitcode($?) != 1
  exit $? if $? != 0
  exit 201
end

puts "exit level for last command is #{$?}"
exit $?