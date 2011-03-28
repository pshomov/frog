#!/usr/bin/env ruby
require 'fileutils'
include FileUtils

def colorize(text, color_code)
  "\e[#{color_code}m#{text}\e[0m"
end

def red(text); colorize(text, "31"); end
def green(text); colorize(text, "32"); end

remove_dir "../output", :force => true

system "xbuild ../Frog.Net.sln /target:rebuild /property:Configuration=Release"
status = $?.exitstatus
if status != 0
	puts(red("No Runz!"))
	exit(status)
end

mkdir "../output"
cp_r "../src/app/Frog.UI.Web", "../output/web"
rm Dir.glob('../output/web/**/*.cs')
rm Dir.glob('../output/web/**/*.csproj')
rm Dir.glob('../output/web/**/*.pidb')
rm Dir.glob('../output/web/**/*vsdoc.js')
rm Dir.glob('../output/web/**/*.user')
rm_r Dir.glob('../output/web/**/obj/*'), :force => true, :secure => true

puts green "Build Runz!"