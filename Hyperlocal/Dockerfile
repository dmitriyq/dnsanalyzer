FROM cznic/knot-resolver:v4.2.0
COPY Hyperlocal/kresd.conf /etc/knot-resolver/
COPY Hyperlocal/start.sh /usr/bin/
ENTRYPOINT ["/usr/bin/start.sh"]