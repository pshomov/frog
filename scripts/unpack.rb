#!/usr/bin/ruby

def _s(command)
  system(command)
  if $? != 0
  	puts(red("Error running '#{command}'!"))
  	exit $?
  end  
end

def colorize(text, color_code)
  "\e[#{color_code}m#{text}\e[0m"
end

def red(text); colorize(text, "31"); end
def green(text); colorize(text, "32"); end

_s("~/fastcgi-mono-server4.sh stop")

_s("rm -rdf web")
_s("7z x out.7z")
_s("chmod -R 755 web")

_s("~/fastcgi-mono-server4.sh start")

puts green "Done!"

exit 0
