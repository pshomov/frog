#!/usr/bin/env ruby
require 'FileUtils'

include FileUtils

LOAD_ARCHIVE = "out.7z"

rm "#{LOAD_ARCHIVE}", :force => true

system "7z a -r #{LOAD_ARCHIVE} ../output/web"
system "ssh -i ~/.ssh/maintainer_id_rsa maintainer@178.239.56.36 mkdir runz"
system "scp -i ~/.ssh/maintainer_id_rsa #{LOAD_ARCHIVE} unpack.rb maintainer@178.239.56.36:~/runz"
system "ssh -i ~/.ssh/maintainer_id_rsa maintainer@178.239.56.36 \"cd runz; chmod +x unpack.rb; ./unpack.rb\""