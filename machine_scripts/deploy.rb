#!/usr/bin/env ruby
require 'FileUtils'

include FileUtils

system "scp fresh_machine.sh root@178.239.56.36:~"
system "ssh root@178.239.56.36 \"chmod +x fresh_machine.sh; ./fresh_machine.sh\""