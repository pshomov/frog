#!/usr/bin/env python
import os
import psutil
import hashlib

total_cpu = 0.0
processes_ids = list()

def add_cpu_times(child):
	global total_cpu, processes_ids
	times = child.get_cpu_times()
	total_cpu += times.user + times.system
	processes_ids.append(child.pid)
    
def kill_children(children):
	for child in children:
		kill_children(child.get_children())
		add_cpu_times(child)

p = psutil.Process(int(os.sys.argv[1]))
kill_children(p.get_children())
add_cpu_times(p)

processes_ids = sorted(processes_ids)
print hashlib.md5(",".join([str(id) for id in processes_ids])).hexdigest(), ' ', total_cpu
