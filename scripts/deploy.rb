#!/usr/bin/env ruby
require 'fileutils'

include FileUtils

LOAD_ARCHIVE = "out.7z"

target = "testbee1.runzhq.com"
rm "#{LOAD_ARCHIVE}", :force => true

system "7z a -r #{LOAD_ARCHIVE} ../output/web"
system "ssh -i ~/.ssh/web_id web@#{target} mkdir runz"
system "scp -i ~/.ssh/web_id #{LOAD_ARCHIVE} unpack.rb web@#{target}:~/runz"
system "ssh -i ~/.ssh/web_id web@#{target} \"cd runz; chmod u=rwx unpack.rb; ./unpack.rb\""