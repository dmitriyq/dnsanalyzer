#!/bin/bash

service bind9 start

echo nameserver 127.0.0.1 > /etc/resolv.conf

tail -f /var/lib/bind/bind.log

exec "$@"

