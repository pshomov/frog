#!/usr/bin/env ruby
require 'FileUtils'

include FileUtils

LOAD_ARCHIVE = "out.7z"

rm "#{LOAD_ARCHIVE}", :force => true

system "7z a -r #{LOAD_ARCHIVE} ../output/web"
system "scp #{LOAD_ARCHIVE} unpack.rb petar@bee.runzhq.com:runz"
system "ssh petar@bee.runzhq.com \"cd runz; chmod +x unpack.rb; ./unpack.rb\""