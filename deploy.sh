#!/bin/sh

BASEPATH=/home/developers/temperature-alarm

# Fetch changes
git -C $BASEPATH pull

# Update frontend
rm -r /var/www/html/*
cp $BASEPATH/frontend/* -r /var/www/html
chown www-data:www-data -R /var/www/html

# Update backend
docker stop api
docker rm api
docker image rm api:latest

docker build --tag api $BASEPATH/backend/Api
docker run -d -p 5000:5000 --volume /home/developers/volume:/app/db --name api --rm api

