apt-get -y update
apt-get -y install git-core curl nginx

useradd -d /home/runner -m runner -p runner
useradd -d /home/runners_boss -m runners_boss -p runners_boss
useradd -d /home/maintainer -m maintainer -p maintainer -r

apt-get install build-essential autoconf automake \
bison \
libcairo2-dev libpango1.0-dev libfreetype6-dev libexif-dev \
libjpeg62-dev libtiff4-dev libgif-dev zlib1g-dev

cp machine_scripts/mono-2.8.2 /usr/local/bin/mono-2.8.2
chmod +x /usr/local/bin/mono-2.8.2.sh
chmod +x machine_scripts/compile_script.sh
./machine_scripts/compile_script.sh

mkdir -p /opt/mono-2.8.2

wget http://ftp.novell.com/pub/mono/sources/mono/mono-2.8.2.tar.bz2
tar xjf mono-2.8.2.tar.bz2
wget http://ftp.novell.com/pub/mono/sources/libgdiplus/libgdiplus-2.8.1.tar.bz2
tar xjf libgdiplus-2.8.1.tar.bz2

cd libgdiplus-2.8.1
./configure --prefix=/opt/mono-2.8.2 --with-pango
make
make install

cd ../mono-2.8.2
./configure --prefix=/opt/mono-2.8.2
make
make install

