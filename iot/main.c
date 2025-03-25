#include <pthread.h>
#include <stdlib.h>
#include <unistd.h>
#include <stdbool.h>
#include <string.h>
#include <stdio.h>
#include <time.h>

#include "brokers/amqp.h"
#include "temperature.h"
#include "device_id.h"

void *watch_temperature(void *arg)
{
	char *device_id = get_device_id();

	printf("Device ID: %s\n", device_id);

	temperature_handle_t temp_handle = init_temperature();

	get_temperature(temp_handle);

	while (true) {
		double temperature = get_temperature(temp_handle);
		size_t timestamp = time(NULL);

		char *format = "{"
			"\"temperature\": %lf,"
			"\"device_id\": \"%s\","
			"\"timestamp\": %zu"
		"}";

		char *str = malloc(snprintf(NULL, 0, format, temperature, device_id, timestamp) + 1);
		sprintf(str, format, temperature, device_id, timestamp);

		amqp_send_message("temperature-logs", str);

		free(str);

		printf("Temperature: %lf\n", temperature);

		sleep(60);
	}

	destroy_device_id(device_id);

	return NULL;
}

void broker_on_connect(void)
{
	pthread_t temperature_thread;
	pthread_create(&temperature_thread, NULL, watch_temperature, NULL);
}

int main(void)
{
	srand(time(NULL));

	init_amqp();

	return EXIT_SUCCESS;
}

