#!/usr/bin/env ruby
require 'fileutils'

include FileUtils

WEB_ARCHIVE = "web.7z"
AGENT_ARCHIVE = "agent.7z"

target = "testbee1.runzhq.com"
rm "#{WEB_ARCHIVE}", :force => true
rm "#{AGENT_ARCHIVE}", :force => true

system "7z a -r #{WEB_ARCHIVE} ../output/web"
system "7z a -r #{AGENT_ARCHIVE} ../output/agent"
system "ssh -i ~/.ssh/web_id web@#{target} mkdir runz"
system "ssh -i ~/.ssh/agent_id agent@#{target} mkdir runz"
system "scp -i ~/.ssh/web_id #{WEB_ARCHIVE} unpack.rb web@#{target}:~/runz"
system "scp -i ~/.ssh/agent_id #{AGENT_ARCHIVE} unpack_agent.rb agent@#{target}:~/runz"
system "ssh -i ~/.ssh/web_id web@#{target} \"cd runz; chmod u=rwx unpack.rb; ./unpack.rb\""
system "ssh -i ~/.ssh/agent_id agent@#{target} \"cd runz; chmod u=rwx unpack_agent.rb; ./unpack_agent.rb\""