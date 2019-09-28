FROM cznic/knot-resolver:v4.2.0
RUN apt-get update -qq && apt-get install -y lua-filesystem && apt-get clean
COPY Hyperlocal/kresd.conf /etc/knot-resolver/
EXPOSE 53/UDP 53/TCP 443/TCP 853/TCP 8453/TCP
ENTRYPOINT ["/usr/sbin/kresd"]
CMD ["-f", "4", "-c", "/etc/knot-resolver/kresd.conf"]
