#!/usr/bin/env ruby
require 'FileUtils'

include FileUtils


system "scp -r ../machine_scripts root@178.239.56.36:~"
system "ssh root@178.239.56.36 \"chmod +x machine_scripts/fresh_machine.sh; machine_scripts/fresh_machine.sh\""