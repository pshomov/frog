require 'rake/clean'
require 'fileutils'
require 'rbconfig'
IS_WINDOWS = (RbConfig::CONFIG['host_os'] =~ /mswin|mingw|cygwin/)

CLOBBER.include('output/*', '**/bin/Debug', '**/bin/Release', '**/obj/Release', '**/obj/Debug')

if IS_WINDOWS
  MSBUILD_PATH = "#{ENV['SYSTEMROOT']}\\Microsoft.NET\\Framework\\v4.0.30319\\msbuild.exe"
else
  MSBUILD_PATH = "xbuild"
end

OUTPUT_PATH = "output"
BUILD_MODE = "Debug"
ACCOUNT = ""
PASSWORD = ""

directory OUTPUT_PATH

APPS_FOLDER = File.join('src', 'app')
TESTS_FOLDER = File.join('src', 'tests')
WEB_APPS = [
    {'src' => APPS_FOLDER, 'project' => 'Frog.UI.Web'},
            ]
CONSOLE_APPS = [
    {'src' => APPS_FOLDER, 'project' => 'Frog.Runner'},
    {'src' => APPS_FOLDER, 'project' => 'Frog.Agent'},
    {'src' => APPS_FOLDER, 'project' => 'Frog.RepositoryTracker'},
    {'src' => APPS_FOLDER, 'project' => 'SaaS.Engine'},
            ]
SERVICE_APPS = [
    {'src' => APPS_FOLDER, 'project' => 'Frog.Agent.Service'},
            ]
ACCEPTANCE_TESTS = [
    {'src' => TESTS_FOLDER, 'project' => 'Frog.Domain.Specs'},
    {'src' => TESTS_FOLDER, 'project' => 'Frog.System.Specs'},
    {'src' => TESTS_FOLDER, 'project' => 'Frog.Domain.IntegrationTests'},
]

task :build => [:clobber, OUTPUT_PATH] do
  compile_base_project(true)
  copy_web_artifacts(WEB_APPS)
  copy_bin_artifacts(ACCEPTANCE_TESTS)
  copy_bin_artifacts(CONSOLE_APPS)
  copy_bin_artifacts(SERVICE_APPS)
end

task :fast_build => [OUTPUT_PATH] do
  compile_base_project(false)
  copy_web_artifacts(WEB_APPS)
  copy_bin_artifacts(ACCEPTANCE_TESTS)
  copy_bin_artifacts(CONSOLE_APPS)
  copy_bin_artifacts(SERVICE_APPS)
end

task :deploy do
  environment = ENV['Env'] || local_environment
  server = {local_environment, 'localhost',development_environment, '', staging_environment, '', production_environment, ''}[environment]

  WEB_APPS.each do |app|
    artifact_folder = File.join(Dir.pwd, OUTPUT_PATH, app['project'])
    select_web_environment("#{OUTPUT_PATH}/#{app['project']}", environment)
    deploy_web_service(artifact_folder, app['deployment'][environment], server)
  end
end

task :select_local_env do
  select_env_for_source_code(local_environment)
end

task :run_acceptance_tests do
  environment = ENV['Env']
  # ENV['PATH'] = "#{ENV['PATH']};#{File.join(Dir.pwd, 'Lib\\Selenium')}"
  ACCEPTANCE_TESTS.each do |test|
    select_bin_environment("#{OUTPUT_PATH}/#{test['artifact_folder']}", environment)
    launch("mono Libs/NUnit/nunit-console-x86.exe #{OUTPUT_PATH}/#{test['project']}/#{test['project']}.dll")
  end
end

def select_env_for_source_code(env)
  WEB_APPS.each {|app| select_web_environment(app['src'], env)}
  ACCEPTANCE_TESTS.each {|app| select_bin_environment(app['src'], env)}
end

def copy_web_artifacts(apps)
  apps.each {|app| copy_web_artifact("#{app['src']}/#{app['project']}", app['project'])}
end

def copy_bin_artifacts(apps)
  apps.each {|app| copy_bin_artifact("#{app['src']}/#{app['project']}", app['project'])}
end

def deploy_web_service(artifacts, ws_path, server)
  # here comes the webdeploy magic
    if server == "localhost" then
      launch("\"Lib\\Microsoft Web Deploy\\msdeploy.exe\" -verb:sync -source:contentPath=\"#{artifacts}\" -dest:contentPath=\"#{ws_path}\" -useCheckSum")
    else
      launch("\"Lib\\Microsoft Web Deploy\\msdeploy.exe\" -verb:sync -source:contentPath=\"#{artifacts}\" -dest:contentPath=\"#{ws_path}\",computerName=#{server},userName=#{ENV['CAMPAIGN_DEPLOYER_USER']},password=#{ENV['CAMPAIGN_DEPLOYER_PASSWORD']} -allowUntrusted -useCheckSum")
    end
end

def launch(cmd)
  sh cmd do |ok, res|
    if !ok
      raise "Error launching #{cmd.to_a}. Result is #{res}."
    end
  end
end

def select_web_environment (folder, env)
  cp_r Dir.glob("#{folder}/_config/#{env}/*"), folder
end

def select_bin_environment (folder, env)
  cp_r Dir.glob("#{folder}/_config/#{env}/*"), folder
end

def compile_base_project (full_rebuild)
  command_line = "#{MSBUILD_PATH} Frog.Net.sln /p:Configuration=#{BUILD_MODE} /target:#{full_rebuild ? 'rebuild' : 'build'} /m"
  if not IS_WINDOWS 
    command_line = "xbuild Frog.Net.sln"
  end
  sh command_line do |ok, res|
    if !ok
      raise "Failed building all projects. Result is #{res}."
    end
  end
end

def copy_web_artifact(web_project, destination)
  web_output_path = "#{OUTPUT_PATH}/#{destination}"
  mkdir web_output_path unless File.directory?(web_output_path)
  cp_r Dir.glob("#{web_project}/*"), web_output_path
  rmtree Dir.glob("#{web_output_path}/**/obj")+(Dir.glob("#{web_output_path}/**/AutoTest.Net"))+Dir.glob("#{web_output_path}/**/_ReSharper.*")+Dir.glob("#{web_output_path}/**/.svn")
  rm all_except(web_output_path, %w(*.swf *.dll *.gif *.jpg *.png *.jpeg *.css *.html *.js *.asmx *.aspx *.master *.xsd *.xml *.config *.cshtml Global.asax *.ashx *.ico *.resx *.pdb *.svc _empty.txt *.py *.rb *.sh *.mdb)), :force => true
  rm only(web_output_path, %w(packages.config *.Publish.xml bin/*.xml)), :force => true
  remove_empty_folders(web_output_path)
end

def copy_bin_artifact(bin_project, destination)
  bin_output_path = "#{OUTPUT_PATH}/#{destination}"
  mkdir bin_output_path unless File.directory?(bin_output_path)
  cp_r Dir.glob("#{bin_project}/bin/#{BUILD_MODE}/*"), "#{bin_output_path}"
  rmtree Dir.glob("#{bin_output_path}/**/obj")+(Dir.glob("#{bin_output_path}/**/AutoTest.Net"))+Dir.glob("#{bin_output_path}/**/_ReSharper.*")+Dir.glob("#{bin_output_path}/**/.svn *.mdb")
  rm all_except(bin_output_path, %w(*.exe *.dll *.xsd *.xml *.config *.mustache *.prn _empty.txt  *.py *.rb *.sh)), :force => true
  rm only(bin_output_path, %w(*.Publish.xml)), :force => true
  remove_empty_folders(bin_output_path)

  copy_all_configuration_settings(bin_output_path, bin_project)
end

def copy_all_configuration_settings(bin_output_path, bin_project)
  #cp_r "#{bin_project}/_config", bin_output_path
end

def all_except (root, allowed)
  els = Dir.glob("#{root}/**/*")
  allowed.each { |el| els = els - Dir.glob("#{root}/**/#{el}") }
  els
end

def only (root, included)
  els = []
  included.each { |el| els = els + Dir.glob("#{root}/**/#{el}") }
  els
end

def remove_empty_folders(root)
  return
  Dir["#{root}/**/*"] \
    .select { |d| File.directory? d } \
    .select { |d| (Dir.entries(d) - %w[ . .. App_Data _config]).empty? } \
    .each { |d| Dir.rmdir d }
  Dir["#{root}/*"] \
    .select { |d| File.directory? d } \
    .select { |d| (Dir.entries(d) - %w[ . .. App_Data _config]).empty? } \
    .each { |d| Dir.rmdir d }
end
