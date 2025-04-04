#include <pthread.h>
#include <stdlib.h>
#include <unistd.h>
#include <stdbool.h>
#include <string.h>
#include <stdio.h>
#include <time.h>

#include "brokers/amqp.h"
#include "devices/temperature.h"
#include "devices/display.h"
#include "devices/buzzer.h"
#include "device_id.h"

void *sound_alarm(void *arg)
{
	buzzer_handle_t buzzer = init_buzzer();

	int delay = 170, length = 600;

	for (int i = 0; i < 5; i++) {
		sound_buzzer(buzzer, delay, length);

		usleep(delay * 2 * length);
	}

	return NULL;
}

void *watch_temperature(void *arg)
{
	// TODO change
	int max_temperature = 27, min_temperature = 18;

	char *device_id = get_device_id();

	printf("Device ID: %s\n", device_id);

	display_handle_t display = init_display();
	display_write_str(display, " ");
	display_set_cursor_pos(display, 0, 1);
	display_write_str(display, "Device.....");
	display_write_str(display, device_id);

	temperature_handle_t temp_handle = init_temperature();
	get_temperature(temp_handle);

	while (true) {
		// Retrieve data
		double temperature = get_temperature(temp_handle);
		size_t timestamp = time(NULL);

		// Send JSON
		char *format = "{"
			"\"temperature\": %lf,"
			"\"device_id\": \"%s\","
			"\"timestamp\": %zu"
		"}";

		char *str = malloc(snprintf(NULL, 0, format, temperature, device_id, timestamp) + 1);
		sprintf(str, format, temperature, device_id, timestamp);

		amqp_send_message("temperature-logs", str);

		free(str);

		// Print on display
		str = malloc(17);
		sprintf(str, "===[ %.1lf\xDF" "C ]===", temperature);

		display_set_cursor_pos(display, 0, 0);
		display_write_str(display, str);

		free(str);

		// Sound alarm if applicable
		if (temperature < min_temperature || temperature > max_temperature) {
			pthread_t alarm_thread;
			pthread_create(&alarm_thread, NULL, sound_alarm, NULL);
		}

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

