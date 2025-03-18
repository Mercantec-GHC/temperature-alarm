#include <mosquitto.h>
#include <stdlib.h>
#include <stdio.h>

#include "config.h"

void on_connect(struct mosquitto *client, void *obj, int rc)
{
	if (rc != 0) {
		fprintf(stderr, "%s\n", mosquitto_connack_string(rc));
		return;
	}

	puts("Connected to " MQTT_IP);
}

void on_message(struct mosquitto *mosq, void *obj, const struct mosquitto_message *message)
{
	printf("Received message on topic %s\n", message->topic);
}

int main(void)
{
	int version[3];
	mosquitto_lib_init();
	mosquitto_lib_version(&version[0], &version[1], &version[2]);
	printf("Using mosquitto library version %i.%i.%i\n", version[0], version[1], version[2]);

	struct mosquitto *mosq = mosquitto_new(NULL, true, NULL);

	mosquitto_connect_callback_set(mosq, on_connect);
	mosquitto_message_callback_set(mosq, on_message);

	mosquitto_username_pw_set(mosq, MQTT_USER, MQTT_PASSWORD);
	mosquitto_connect(mosq, MQTT_IP, MQTT_PORT, 60);

	mosquitto_loop_forever(mosq, -1, 1);

	mosquitto_destroy(mosq);
	mosquitto_lib_cleanup();

	return EXIT_SUCCESS;
}

