#!/bin/bash

if [ -z "$2" ] ; then
  echo 'manual restart'
else
  cd $2
fi

echo 'shut down in process' $1
./shutdown.sh $1

#if [ ! -z "$2" ] || [ "$1" eq "common" ]
#then
  echo 'wati for 2 sec to kill the process'
  sleep 2
#fi

if [ $# -eq 0 ] ; then
    rm -f log/*.txt
    rm -f pid/*.pid
else
    rm -f log/log_$1.txt
    rm -f pid/$1.pid
fi

echo 'start up in process'
./startup.sh $1
echo 'completed'