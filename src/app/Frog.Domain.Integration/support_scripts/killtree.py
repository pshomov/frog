#!/usr/bin/env python
import os
import psutil

def kill_children(children):
    for child in children:
        kill_children(child.get_children())
        print "Killing child process %s" % child.pid
        child.kill()

print "About to take down process %s" % os.sys.argv[1]
p = psutil.Process(int(os.sys.argv[1]))
kill_children(p.get_children())
p.kill()
