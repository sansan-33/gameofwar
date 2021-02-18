#!/bin/bash
if [ $# -eq 0 ] || [ $1 == "server" ]; then
    kill $(cat pid/server.pid)
fi
