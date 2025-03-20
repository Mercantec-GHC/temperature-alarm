#include <pthread.h>
#include <stdlib.h>
#include <unistd.h>
#include <stdbool.h>
#include <string.h>
#include <stdio.h>

#include "mqtt.h"
#include "temperature.h"

void *watch_temperature(void *arg)
{
	temperature_handle_t temp_handle = init_temperature();

	get_temperature(temp_handle);

	while (true) {
		double temperature = get_temperature(temp_handle);

		char *str = malloc(snprintf(NULL, 0, "%lf", temperature) + 1);
		sprintf(str, "%lf", temperature);

		mqtt_send_message("/temperature", str);

		free(str);

		printf("Temperature: %lf\n", temperature);

		sleep(60);
	}

	return NULL;
}

void mqtt_on_connect(void)
{
	pthread_t temperature_thread;
	pthread_create(&temperature_thread, NULL, watch_temperature, NULL);
}

int main(void)
{
	init_mqtt();

	return EXIT_SUCCESS;
}

