Runz runtime requirements:
--------------------------

* Git - it is good for you ;)
* Ruby - it is used as a platform independent scripting language to launch git
* Python - Fabric is the bomb!
* RabbitMQ - to do the message pushing
  * environment variable RUNZ\_RABITMQ\_SERVER should have the host that has the rabbit mq server
* Bash - there are a few scripts we really need to get rid of

---

Runz development requirements:
------------------------------
* environment variable RUNZ\_TEST\_RABBITMQ\_SERVER should have the host that has the rabbit mq server for integration testing
* environment variable RUNZ\_ACCEPTANCE\_MODE should be set to ACCEPTANCE to run the Web UI in acceptance mode (necessary if you need to run the acceptance tests)


On OS X brew can help you with these I believe.
On Windows you will need cygwin to the bashing
On Linux we should be using the scripts that we are building