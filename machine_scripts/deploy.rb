#!/usr/bin/env ruby
require 'fileutils'
require 'system_common.rb'
include FileUtils


if ENV["MONO_VERSION"].nil? 
  puts red("MONO_VERSION not specified")
  exit 2
end
if ENV["LIBGDI_VERSION"].nil? 
  puts red("LIBGDI_VERSION not specified")
  exit 2
end

remove_dir "../tmp", :force => true
mkdir "../tmp"
script_folder = File.dirname(__FILE__)
Dir.foreach(script_folder) do |script|
  if !script.starts_with?("\\.")
    puts "processing #{script}"
    _s("erb #{script_folder}/#{script} > ../tmp/#{script}")
  end
end
_s "ssh -i ~/.ssh/root_id_rsa root@178.239.56.36 \"rm -rdf machine_scripts\""
_s "scp -i ~/.ssh/root_id_rsa -r ../tmp root@178.239.56.36:~/machine_scripts"
_s "ssh -i ~/.ssh/root_id_rsa root@178.239.56.36 \"chmod +x machine_scripts/*.sh machine_scripts/*.rb; machine_scripts/fresh_machine.sh\""