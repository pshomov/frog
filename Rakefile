require 'rake/clean'
require 'fileutils'

is_windows = (RbConfig::CONFIG['host_os'] =~ /mswin|mingw|cygwin/)

if is_windows
  require 'win32/service'
  include Win32
end

CLOBBER.include('output/**/*', '**/bin/Debug', '**/bin/Release')

MSBUILD_PATH = "#{ENV['SYSTEMROOT']}\\Microsoft.NET\\Framework\\v4.0.30319\\msbuild.exe"
OUTPUT_PATH = "output"
BUILD_MODE = "Debug"
ACCOUNT = "EIMDP020\\v.aji"
PASSWORD = "Er46Hyu7"
SERVER = "10.5.72.34"
PSEXEC="Binaries\\paexec \\\\#{SERVER} -u #{ACCOUNT} -p #{PASSWORD}"
INSTALLUTIL = 'c:\windows\microsoft.net\framework\v2.0.50727\installutil'

directory OUTPUT_PATH

staging_environment = 'Staging'
development_environment = 'Dev'
production_environment = 'Production'

task :build => [:clobber, OUTPUT_PATH] do
  select_web_environment('fylgibref/fylgibref', development_environment)
  select_bin_environment('fylgibref.Service', development_environment)
  select_bin_environment('fylgibref/fylgibref.Contract.Tests', development_environment)
  select_bin_environment('fylgibref.AcceptanceTests', development_environment)
  select_bin_environment('fylgibref/fylgibref.E2E.Tests', development_environment)
  select_bin_environment('fylgibref.System.Tests', development_environment)
  compile_all_projects()
  copy_web_artifact('fylgibref/fylgibref', 'fylgibref')
  copy_bin_artifact("fylgibref.Service", 'fylgibref.Service')
  copy_bin_artifact("fylgibref.AcceptanceTests", 'fylgibref.AcceptanceTests')
  copy_bin_artifact("fylgibref.Unit.Tests", 'fylgibref.UnitTests')
  copy_bin_artifact("fylgibref.System.Tests", 'fylgibref.SystemTests')
  copy_bin_artifact("fylgibref/fylgibref.Contract.Tests", 'fylgibref.ContractTests')
  copy_bin_artifact("fylgibref/fylgibref.E2E.Tests", 'fylgibref.E2ETests')
end


def deploy_system_service(service_name, artifact_folder, environment, remote_artifact_folder, service_local_artifact_path, service_local_executable)
  stop_service(service_name)
  uninstall_service(service_name, service_local_executable)
  cleanup_remote_path(service_local_artifact_path)
  install_service(artifact_folder, remote_artifact_folder, service_name, service_local_executable)
  start_service service_name
end

task :deploy do
  environment = ENV['Env']
  ws_env_path = {development_environment => 'WebServicesDEV', staging_environment => 'WebServicesTEST', production_environment => 'WebServices'}[environment]
  service_name = {development_environment => 'FylgibrefWorkerDev', staging_environment => 'FylgibrefWorkerStaging', production_environment => 'FylgibrefWorker'}[environment]
  artifact_folder = 'fylgibref.Service'
  service_local_artifact_path = "c:\\Sprettur\\#{environment}\\#{artifact_folder}"
  service_local_executable = "#{service_local_artifact_path}\\fylgibref.Service.exe"
  remote_artifact_folder = "\\\\#{SERVER}\\c$\\Sprettur\\#{environment}\\#{artifact_folder}"

  select_web_environment("#{OUTPUT_PATH}/fylgibref", environment)
  select_bin_environment("#{OUTPUT_PATH}/fylgibref.Service", environment)

  run_db_migrations environment

  deploy_system_service(service_name, artifact_folder, environment, remote_artifact_folder, service_local_artifact_path, service_local_executable)
  deploy_web_service("fylgibref", ws_env_path)
  Statsd.new('108.171.189.152').increment("#{environment}.flytjandi.deploy.webservice")
end

task :unit_test do
  environment = ENV['Env']
  select_bin_environment("#{OUTPUT_PATH}/fylgibref.ContractTests", environment)
  select_bin_environment("#{OUTPUT_PATH}/fylgibref.SystemTests", environment)
  launch('binaries\NUnit\nunit-console-x86.exe output\fylgibref.UnitTests\fylgibref.Unit.Tests.dll output\fylgibref.SystemTests\fylgibref.System.Tests.dll output\fylgibref.ContractTests\fylgibref.Contract.Tests.dll')
end

task :acceptance_test do
  environment = ENV['Env']
  select_bin_environment("#{OUTPUT_PATH}/fylgibref.AcceptanceTests", environment)
  launch('binaries\NUnit\nunit-console-x86.exe output\fylgibref.AcceptanceTests\fylgibref.AcceptanceTests.dll')
end

def compile_all_projects
  sh "#{MSBUILD_PATH} VMKerfi.sln /p:Configuration=#{BUILD_MODE} /target:rebuild" do |ok, res|
    if !ok
      raise "Failed building all projects. Result is #{res}."
    end
  end
end

def run_db_migrations(environment)
  ENV['RAILS_ENV'] = environment
  Rake::Task['db:migrate'].invoke({'RAILS_ENV', environment})
end

def launch(cmd)
  sh cmd do |ok, res|
    if !ok
      raise "Error launching #{cmd.to_a}. Result is #{res}."
    end
  end
end

def copy_all_configuration_settings(bin_output_path, bin_project)
  cp_r("#{bin_project}/config", bin_output_path) if File.exists? "#{bin_project}/config"
end

def copy_bin_artifact(bin_project, destination)
  bin_output_path = "#{OUTPUT_PATH}/#{destination}"
  cp_r "#{bin_project}/bin/#{BUILD_MODE}", "#{bin_output_path}"
  rmtree Dir.glob("#{bin_output_path}/**/obj")+(Dir.glob("#{bin_output_path}/**/AutoTest.Net"))+Dir.glob("#{bin_output_path}/**/_*")
  rm all_except(bin_output_path, %w(*.exe *.dll *.xsd *.xml *.config *.mustache *.prn)), :force => true
  rm only(bin_output_path, %w(*.Publish.xml)), :force => true
  remove_empty_folders(bin_output_path)

  copy_all_configuration_settings(bin_output_path, bin_project)
end

def copy_web_artifact(web_project, destination)
  cp_r web_project, "#{OUTPUT_PATH}/#{destination}"
  web_output_path = "#{OUTPUT_PATH}/#{destination}"
  rmtree Dir.glob("#{web_output_path}/**/obj")+(Dir.glob("#{web_output_path}/**/AutoTest.Net"))+Dir.glob("#{web_output_path}/**/_*")
  rm all_except(web_output_path, %w(*.dll *.gif *.jpg *.png *.jpeg *.css *.html *.asmx *.xsd *.xml *.config *.mustache)), :force => true
  rm only(web_output_path, %w(*.Publish.xml)), :force => true
  remove_empty_folders(web_output_path)
end

def all_except (root, allowed)
  els = Dir.glob("#{root}/**/*")
  allowed.each { |el| els = els - Dir.glob("#{root}/**/#{el}") }
  els
end

def select_web_environment (folder, env)
  cp_r "#{folder}/config/#{env}/.", folder
end

def select_bin_environment (folder, env)
  cp_r "#{folder}/config/#{env}/.", folder
  built_folder = "#{folder}/bin/#{BUILD_MODE}"
  mkdir_p built_folder
  cp_r "#{folder}/config/#{env}/.", built_folder
end

def only (root, included)
  els = []
  included.each { |el| els = els + Dir.glob("#{root}/**/#{el}") }
  els
end

def remove_empty_folders(root)
  Dir["#{root}/**/*"] \
    .select { |d| File.directory? d } \
    .select { |d| (Dir.entries(d) - %w[ . .. ]).empty? } \
    .each { |d| Dir.rmdir d }
end

def mount_web_drive
  launch "net use \\%SERVER%\c$ #{PASSWORD} /USER:#{ACCOUNT}"
end

def stop_service(service_name)
  if Service.exists?(service_name, SERVER) and (Service.status(service_name, SERVER).current_state == 'running')
    launch "#{PSEXEC} net stop #{service_name}"
  end
end

def start_service(service_name)
  launch "#{PSEXEC} net start #{service_name}"
end

def uninstall_service(service_name, path_to_executable)
  if Service.exists? service_name, SERVER
    launch "#{PSEXEC} #{INSTALLUTIL} /ServiceName=#{service_name} /u #{path_to_executable}"
  end
end

def install_service(artifacts, destination, service_name, path_to_executable)
  mkdir_p destination
  launch  "xcopy /s/y #{OUTPUT_PATH}\\#{artifacts} #{destination}"
  launch "#{PSEXEC} #{INSTALLUTIL} /ServiceName=#{service_name} #{path_to_executable}"
end

def deploy_web_service(artifacts, ws_path)
  temp_section = rand(1000000)
  tmp_path = "\\\\#{SERVER}\\c$\\Sprettur\\Temp\\#{temp_section}"
  mkdir_p tmp_path
  launch  "xcopy /s/y #{OUTPUT_PATH}\\#{artifacts} #{tmp_path}"
  cleanup_remote_path "c:\\inetpub\\wwwroot\\fylgibref.eimskip.net\\#{ws_path}\\fylgibref"
  remotely_cmd SERVER, "(mkdir c:\\inetpub\\wwwroot\\fylgibref.eimskip.net\\#{ws_path}\\fylgibref) & (xcopy /s /y c:\\sprettur\\temp\\#{temp_section} c:\\inetpub\\wwwroot\\fylgibref.eimskip.net\\#{ws_path}\\fylgibref)"
end

def remotely(server, command)
  launch "#{PSEXEC} #{command}"
end

def remotely_cmd(server, command)
  File.open('launchme.cmd', 'w') {|f| f.write(command)}
  launch "#{PSEXEC} -c launchme.cmd"
  rm 'launchme.cmd'
end

def cleanup_remote_path(local_artifact_path)
  remotely_cmd SERVER, "if exist #{local_artifact_path} rmdir /s /q #{local_artifact_path}"
end
