FILES=main.c brokers/mqtt.c brokers/amqp.c devices/temperature.c devices/display.c devices/buzzer.c device_id.c

# libgpiod version must be < 2.0

all: $(FILES)
	$(CC) -lmosquitto -lrabbitmq -pthread -li2c -lgpiod $(FILES)

