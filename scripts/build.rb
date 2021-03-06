#!/usr/bin/env ruby
require 'fileutils'
include FileUtils

def colorize(text, color_code)
  "\e[#{color_code}m#{text}\e[0m"
end

def red(text); colorize(text, "31"); end
def green(text); colorize(text, "32"); end

remove_dir "../output", :force => true
build_mode="Release"
system "xbuild ../Frog.Net.sln /target:rebuild /property:Configuration=#{build_mode}"
status = $?.exitstatus
if status != 0
	puts(red("No Runz!"))
	exit(status)
end

mkdir "../output"
cp_r "../src/app/Frog.UI.Web", "../output/web"
cp_r "../src/app/Web", "../output/webz"
cp_r "../src/app/Frog.Agent/bin/#{build_mode}", "../output/agent"
cp_r "../src/app/Frog.RepositoryTracker/bin/#{build_mode}", "../output/repotracker"
cp_r "../src/app/SaaS.Engine/bin/#{build_mode}", "../output/projections"
cp_r "../src/app/Frog.Agent.Service/bin/#{build_mode}", "../output/agentservice"
cp_r "../src/app/Frog.Runner/bin/#{build_mode}", "../output/runner"
rm Dir.glob('../output/web/**/*.cs')
rm Dir.glob('../output/web/**/*.csproj')
rm Dir.glob('../output/web/**/*.pidb')
rm Dir.glob('../output/web/**/*vsdoc.js')
rm Dir.glob('../output/web/**/*.user')
rm_r Dir.glob('../output/web/**/obj/*'), :force => true, :secure => true

puts green "Build Runz!"
