#!/bin/bash

echo nameserver 127.0.0.1 > /etc/resolv.conf
touch /var/cache/bind/managed-keys.bind
chmod -R 777 /var/cache/bind/managed-keys.bind
chmod -R 0777 /var/cache/bind/managed-keys.bind
chown bind:bind /var/cache/bind/managed-keys.bind
service bind9 start
tail -f /var/lib/bind/bind.log

exec "$@"
