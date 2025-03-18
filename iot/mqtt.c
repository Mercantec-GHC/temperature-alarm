#include <mosquitto.h>
#include <stdio.h>
#include <string.h>
#include <stdbool.h>

#include "config.h"

struct mosquitto *mosq;

void mqtt_on_connect(void);

void on_connect(struct mosquitto *client, void *obj, int rc)
{
	if (rc != 0) {
		fprintf(stderr, "%s\n", mosquitto_connack_string(rc));
		return;
	}

	puts("Connected to " MQTT_IP);

	mqtt_on_connect();
}

void on_message(struct mosquitto *mosq, void *obj, const struct mosquitto_message *message)
{
	printf("Received message on topic %s\n", message->topic);
}

void mqtt_send_message(char *topic, char *message)
{
	int result = mosquitto_publish(mosq, NULL, topic, strlen(message), message, 0, false);
	if (result != MOSQ_ERR_SUCCESS) {
		fprintf(stderr, "Error sending message: %d\n", result);
	}
}

void init_mqtt(void)
{
	int version[3];
	mosquitto_lib_init();
	mosquitto_lib_version(&version[0], &version[1], &version[2]);
	printf("Using mosquitto library version %i.%i.%i\n", version[0], version[1], version[2]);

	mosq = mosquitto_new(NULL, true, NULL);

	mosquitto_connect_callback_set(mosq, on_connect);
	mosquitto_message_callback_set(mosq, on_message);

	mosquitto_username_pw_set(mosq, MQTT_USER, MQTT_PASSWORD);
	mosquitto_connect(mosq, MQTT_IP, MQTT_PORT, 60);

	mosquitto_loop_forever(mosq, -1, 1);

	mosquitto_destroy(mosq);
	mosquitto_lib_cleanup();
}

