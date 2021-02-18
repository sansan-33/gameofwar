#!/bin/bash

sleep 20


if [ $# -eq 0 ] || [ $1 == "7777" ]; then
    	nohup open -o log/log_out_7777.txt /Users/ytwong/github/bellum/server/macserver_7777.app > log/log_server_7777.txt   2>&1 &
	echo $!  > pid/server_7777.pid
fi

if [ $# -eq 0 ] || [ $1 == "7778" ]; then
    	nohup open -o log/log_out_7778.txt /Users/ytwong/github/bellum/server/macserver_7778.app > log/log_server_7778.txt   2>&1 &
	echo $!  > pid/server_7778.pid
fi

if [ $# -eq 0 ] || [ $1 == "7779" ]; then
    	nohup open -o log/log_out_7778.txt /Users/ytwong/github/bellum/server/macserver_7779.app > log/log_server_7779.txt   2>&1 &
	echo $!  > pid/server_7779.pid
fi