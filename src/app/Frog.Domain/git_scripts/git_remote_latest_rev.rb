#!/usr/bin/env ruby

system "git ls-remote #{ARGV[0]} master"
exit $?