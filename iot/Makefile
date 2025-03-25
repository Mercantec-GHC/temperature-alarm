all: main.c brokers/mqtt.c brokers/amqp.c temperature.c device_id.c
	$(CC) -lmosquitto -lrabbitmq -lpthread -li2c main.c brokers/mqtt.c brokers/amqp.c temperature.c device_id.c

