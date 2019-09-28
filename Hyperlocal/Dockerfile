FROM cznic/knot-resolver:v4.2.0
COPY Hyperlocal/kresd.conf /etc/knot-resolver/
COPY Hyperlocal/start.sh /usr/bin/
EXPOSE 53/UDP 53/TCP 443/TCP 853/TCP 8453/TCP
ENTRYPOINT ["/usr/bin/start.sh"]
CMD ["-f", "4", "-c", "/etc/knot-resolver/kresd.conf"]