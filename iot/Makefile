FILES=main.c brokers/mqtt.c brokers/amqp.c devices/temperature.c devices/display.c device_id.c

all: $(FILES)
	$(CC) -lmosquitto -lrabbitmq -lpthread -li2c $(FILES)

