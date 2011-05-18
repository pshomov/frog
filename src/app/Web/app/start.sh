#!/bin/bash

gunicorn -b unix:/tmp/gunicorn.sock --log-file=webz.log project:application