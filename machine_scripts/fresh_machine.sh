apt-get -y update
apt-get -y install git-core curl nginx

useradd -d /home/runner -m runner -p runner
useradd -d /home/runners_boss -m runners_boss -p runners_boss
useradd -d /home/maintainer -m maintainer -p maintainer -r
