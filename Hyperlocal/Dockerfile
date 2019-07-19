FROM ubuntu:18.04
RUN apt update && apt upgrade -y && apt install bind9 dnsutils -y && apt clean
COPY Hyperlocal/* /etc/bind/
COPY Hyperlocal/start.sh /usr/local/bin/
EXPOSE 53
EXPOSE 53/udp
ENTRYPOINT ["start.sh"]
