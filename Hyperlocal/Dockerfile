FROM archlinux/base
RUN pacman -Sy --noconfirm bind bind-tools nano && pacman -Syu --noconfirm
COPY Hyperlocal/named.conf /etc/
COPY Hyperlocal/start.sh /usr/local/bin/
COPY Hyperlocal/rndc.conf /etc/
COPY Hyperlocal/rndc.key /etc/
EXPOSE 53
EXPOSE 53/udp
RUN sed -i -e 's/\r$//' /usr/local/bin/start.sh
ENTRYPOINT ["/usr/local/bin/start.sh"]
