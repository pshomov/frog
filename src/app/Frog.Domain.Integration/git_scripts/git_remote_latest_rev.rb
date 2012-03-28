#!/usr/bin/env ruby
$stderr.puts("remote_latest repo url is #{ARGV[0]}")
system "git ls-remote \"#{ARGV[0]}\" master"
exit $?