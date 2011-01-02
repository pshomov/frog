#!/usr/bin/env ruby

Dir.chdir ARGV[0]+'/'+ARGV[1]
open('test.txt', 'w') do |fd|
  fd.puts 'safasfdasfasfdasfdasfd'
end
system 'git add test.txt'
exit $? if $? != 0
system 'git commit -m "commit number two"'
exit $? if $? != 0
