#!/usr/bin/env ruby
require 'fileutils'

include FileUtils

cp_r ARGV[1], ARGV[0]
Dir.chdir ARGV[0]
system 'git add .'
exit $? if $? != 0
system 'git commit -m "wind of change"'
exit $? if $? != 0
