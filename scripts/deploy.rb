#!/usr/bin/env ruby
require 'fileutils'

include FileUtils

LOAD_ARCHIVE = "out.7z"

target = "testbee1.runzhq.com"
rm "#{LOAD_ARCHIVE}", :force => true

system "7z a -r #{LOAD_ARCHIVE} ../output/web"
system "ssh -i ~/.ssh/maintainer_id_rsa maintainer@#{target} mkdir runz"
system "scp -i ~/.ssh/maintainer_id_rsa #{LOAD_ARCHIVE} unpack.rb maintainer@#{target}:~/runz"
system "ssh -i ~/.ssh/maintainer_id_rsa maintainer@#{target} \"cd runz; chmod u=rx unpack.rb; ./unpack.rb\""