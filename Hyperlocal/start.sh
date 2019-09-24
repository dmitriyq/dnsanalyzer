#!/bin/bash

chmod -R 0777 /var/log
chmod -R 0777 /var/named
chmod -R 0777 /etc
echo "" > /var/log/named.log

chmod -R 0777 /var/log

/usr/bin/named -f -u named -4 &

echo nameserver 127.0.0.1 > /etc/resolv.conf

tail -f /var/log/named.log

exec "$@"