import os
import json
import hashlib

from riak import RiakClient
from riak import RiakPbcTransport
riak_client = RiakClient(os.getenv('RUNZ_RIAK_HOST') or '127.0.0.1', 8087, transport_class=RiakPbcTransport)

proj_bucket = riak_client.bucket('projects')
for key in proj_bucket.get_keys():
	proj_bucket.get_binary(key).delete()
