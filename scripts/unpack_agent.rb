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

_s("~/runz-agent.sh stop")

_s("rm -rdf agent")
_s("7z x agent.7z")
_s("chmod -R 755 agent")

_s("~/runz-agent.sh start")

puts green "Done!"

exit 0
