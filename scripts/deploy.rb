#!/usr/bin/env ruby
require 'FileUtils'

include FileUtils

rm "out.7z", :force => true

system "7za a -r out.7z ../output/web"
system "scp out.7z unpack.sh petar@bee.runzhq.com:runz"
system "ssh petar@bee.runzhq.com \"cd runz; chmod +x unpack.sh; ./unpack.sh\""