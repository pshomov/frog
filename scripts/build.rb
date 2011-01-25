#!/usr/bin/env ruby
require 'FileUtils'

include FileUtils

def colorize(text, color_code)
  "\e[#{color_code}m#{text}\e[0m"
end

def red(text); colorize(text, "31"); end
def green(text); colorize(text, "32"); end

remove_dir "../output", :force => true

system "xbuild ../Frog.Net.sln /target:rebuild"
puts(red("No Runz!")) && (exit $?) if $? != 0

mkdir "../output"
cp_r "../src/app/Frog.UI.Web", "../output/web"
rm Dir.glob('../output/web/**/*.cs')
rm Dir.glob('../output/web/**/*.csproj')
rm Dir.glob('../output/web/**/*.pidb')
rm_r Dir.glob('../output/web/**/obj/*'), :force => true, :secure => true

puts green "Build Runz!"