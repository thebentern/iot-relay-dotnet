FROM influxdb:1.5.4-alpine

WORKDIR /app
COPY entrypoint.sh ./
RUN chmod u+x entrypoint.sh

ENTRYPOINT ["sh", "/app/entrypoint.sh"]