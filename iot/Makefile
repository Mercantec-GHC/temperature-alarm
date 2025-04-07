FILES=main.c brokers/mqtt.c brokers/amqp.c devices/temperature.c devices/display.c devices/buzzer.c device_properties.c

# libgpiod version must be < 2.0

# apt install libmosquitto-dev librabbitmq-dev libi2c-dev libgpiod-dev libcjson-dev

all: $(FILES)
	$(CC) -lmosquitto -lrabbitmq -pthread -li2c -lgpiod -lcjson $(FILES)

