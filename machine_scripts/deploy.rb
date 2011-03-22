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

target = ARGV[0]

remove_dir "../tmp", :force => true
mkdir "../tmp"
script_folder = File.dirname(__FILE__)
Dir.foreach(script_folder) do |script|
  if !script.starts_with?("\\.")
    if script.ends_with?("\\.erb")
      puts "processing #{script}"
      _s("erb #{script_folder}/#{script} > ../tmp/#{script[0..-5]}")
    else
      puts "copying #{script}"
      cp (script, "../tmp")
    end
  end
end
_s "ssh -i ~/.ssh/root_id_rsa root@#{target} \"rm -rdf machine_scripts\""
_s "scp -i ~/.ssh/root_id_rsa -r ../tmp root@#{target}:~/machine_scripts"
_s "ssh -i ~/.ssh/root_id_rsa root@#{target} \"chmod +x machine_scripts/*.sh machine_scripts/*.rb; machine_scripts/fresh_machine.sh\""