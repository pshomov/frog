#!/usr/bin/env ruby
require 'fileutils'
include FileUtils
require 'digest/md5'
cache_sub_folder = Digest::MD5.hexdigest(ARGV[0])
cache_folder = ".repos_cache/#{cache_sub_folder}"
current_dir = Dir.pwd
if not File.exists?(cache_folder)
	mkdir_p cache_folder
	Dir.chdir cache_folder

	system 'git init .'
	exit $? if $? != 0
	system "git remote add origin \"#{ARGV[0]}\""
	exit $? if $? != 0
	system 'git remote update'
	exit $? if $? != 0
	system 'git checkout -b master --track origin/master'
	exit $? if $? != 0
else
	Dir.chdir cache_folder
	system "git pull origin master"
	exit $? if $? != 0
end
system "git checkout #{ARGV[1]}"
exit $? if $? != 0
system "git  log -1 --format=%s  #{ARGV[1]}"
exit $? if $? != 0
Dir.chdir current_dir
cp_r "#{cache_folder}/.", ARGV[2]
remove_dir "#{ARGV[2]}/.git"
exit 0
