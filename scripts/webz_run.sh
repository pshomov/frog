#!/bin/bash

PATH_TO_ME=`dirname $0`
$PATH_TO_ME/../../pond/web_server/setup_virtenv.sh
source webz_virtenv/bin/activate
cd $PATH_TO_ME/../src/app/Web/app
gunicorn  -b unix:/tmp/gunicorn.sock --log-file=webz.log project:application
