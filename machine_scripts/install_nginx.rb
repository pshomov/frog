#!/usr/bin/env ruby

require 'system_common.rb'

script_folder = File.dirname(__FILE__)
_s "cp #{script_folder}/default /etc/nginx/sites-enabled/default"
_s "/etc/init.d/nginx restart"