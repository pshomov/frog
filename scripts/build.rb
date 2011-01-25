#!/usr/bin/env ruby
require 'FileUtils'

include FileUtils

remove_dir "../output", :force => true

system "xbuild ../Frog.Net.sln /target:rebuild"
exit $? if $? != 0

mkdir "../output"
cp_r "../src/app/Frog.UI.Web", "../output/web"
rm Dir.glob('../output/web/**/*.cs')
rm Dir.glob('../output/web/**/*.csproj')
rm Dir.glob('../output/web/**/*.pidb')
rm_r Dir.glob('../output/web/**/obj/*'), :force => true, :secure => true

puts "Build Runz!"