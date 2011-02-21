#!/bin/bash
<% @mono_ver =  ENV['MONO_VERSION']%>
<% @libgdi_ver =  ENV['LIBGDI_VERSION']%>

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
