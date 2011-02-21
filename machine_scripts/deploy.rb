#!/usr/bin/env ruby
require 'FileUtils'

include FileUtils
class String
  def starts_with?(characters)
      self.match(/^#{characters}/) ? true : false
  end
end

def _s(command)
  system(command)
  if $? != 0
  	puts(red("Error running '#{command}'!"))
  	exit $?
  end  
end

def colorize(text, color_code)
  "\e[#{color_code}m#{text}\e[0m"
end

def red(text); colorize(text, "31"); end
def green(text); colorize(text, "32"); end

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
_s "ssh -i ~/.ssh/root_id_rsa root@178.239.56.36 \"chmod +x machine_scripts/*.sh; machine_scripts/fresh_machine.sh\""