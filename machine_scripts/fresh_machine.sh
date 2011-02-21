<% @mono_ver =  ENV['MONO_VERSION']%>
<% @libgdi_ver =  ENV['LIBGDI_VERSION']%>
apt-get -y update
apt-get -y install git-core curl nginx ruby

useradd -d /home/runner -m runner -p runner
useradd -d /home/runners_boss -m runners_boss -p runners_boss
useradd -d /home/maintainer -m maintainer -p maintainer -r

apt-get install build-essential autoconf automake \
bison \
libcairo2-dev libpango1.0-dev libfreetype6-dev libexif-dev \
libjpeg62-dev libtiff4-dev libgif-dev zlib1g-dev

cp machine_scripts/mono /usr/local/bin/mono-<%= @mono_ver %>
chmod +x /usr/local/bin/mono-<%= @mono_ver %>.sh
chmod +x machine_scripts/compile_script.sh
machine_scripts/compile_script.sh

mkdir -p /opt/mono-<%= @mono_ver %>

wget http://ftp.novell.com/pub/mono/sources/mono/mono-<%= @mono_ver %>.tar.bz2
tar xjf mono-<%= @mono_ver %>.tar.bz2
wget http://ftp.novell.com/pub/mono/sources/libgdiplus/libgdiplus-<%= @libgdi_ver %>.tar.bz2
tar xjf libgdiplus-<%= @libgdi_ver %>.tar.bz2

cd libgdiplus-<%= @libgdi_ver %>
./configure --prefix=/opt/mono-<%= @libgdi_ver %> --with-pango
make
make install

cd ../mono-<%= @mono_ver %>
./configure --prefix=/opt/mono-<%= @mono_ver %>
make
make install

