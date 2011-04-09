from fabric.api import *
from fabric.contrib.console import confirm
from fabric.operations import sudo

def install_packages():
    put("rubygems-1.7.2.tgz")
    run("tar -xf rubygems-1.7.2.tgz")
    with cd("rubygems-1.7.2"):
        sudo("ruby setup.rb")
    sudo("apt-get -y install libxml2 libxml2-dev")
    sudo("apt-get -y install libxslt-ruby1.8 libxslt1-dev")
    sudo("apt-get -y install rake")
    sudo("gem install bundler rack")
    sudo("gem install therubyracer")
    
    