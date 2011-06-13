import os
import bobo
import webob
import json
import hashlib

from riak import RiakClient
from riak import RiakPbcTransport
riak_client = RiakClient(os.getenv('RUNZ_RIAK_HOST') or '127.0.0.1', 8087, transport_class=RiakPbcTransport)

def generate_id(posted):
    return hashlib.md5(posted['projecturl']).hexdigest()

@bobo.post('/projects', content_type="application/json")
def post_project(bobo_request):    
    # decode JSON posted object - it will raise an error if it doesn't work
    posted = json.loads(bobo_request.body)
    if not posted.has_key('projecturl'):
        raise ValueError("Project url required")
    # add the revision key
    posted['revision'] = 0
    proj_bucket = riak_client.bucket('projects')
    stored_obj = proj_bucket.new(generate_id(posted), posted)
    stored_obj.store()
    return {'result': 'success'}
    
    
@bobo.query('/projects/:project_id', method='GET', content_type="application/json")
def get_project(project_id):
    proj_bucket = riak_client.bucket('projects')
    return proj_bucket.get(project_id).get_data()
    

application = bobo.Application(bobo_resources=__name__)