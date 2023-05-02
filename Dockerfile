FROM node:12.14

ARG port

ENV TINI_VERSION v0.19.0
ADD https://github.com/krallin/tini/releases/download/${TINI_VERSION}/tini /tini
RUN chmod +x /tini

EXPOSE ${port}

RUN apt-get update && apt-get upgrade -y
RUN apt-get install lame -y

USER node
RUN mkdir -p /home/node/app
WORKDIR /home/node/app

COPY --chown=node:node package.json .
COPY --chown=node:node yarn.lock .

RUN yarn && yarn cache clean

COPY --chown=node:node . .

ENV NODE_ENV=local

ENTRYPOINT ["/tini", "--"]
CMD [ "yarn", "start:dev" ]
