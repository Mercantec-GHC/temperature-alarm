#include <pthread.h>
#include <stdlib.h>
#include <unistd.h>
#include <stdbool.h>
#include <string.h>
#include <stdio.h>
#include <time.h>
#include <math.h>
#include <cjson/cJSON.h>

#include "brokers/amqp.h"
#include "devices/temperature.h"
#include "devices/display.h"
#include "devices/buzzer.h"
#include "device_properties.h"

void *sound_alarm(void *arg)
{
	buzzer_handle_t buzzer = init_buzzer();

	int delay = 170, length = 600;

	for (int i = 0; i < 5; i++) {
		sound_buzzer(buzzer, delay, length);

		usleep(delay * 2 * length);
	}

	destroy_buzzer(buzzer);

	return NULL;
}

void *watch_temperature(void *arg)
{
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
		double min_temperature = get_min_temperature();
		double max_temperature = get_max_temperature();
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
		if (
			min_temperature != NAN && max_temperature != NAN &&
			(temperature < min_temperature || temperature > max_temperature)
		) {
			pthread_t alarm_thread;
			pthread_create(&alarm_thread, NULL, sound_alarm, NULL);
		}

		printf("Temperature: %lf\n", temperature);

		sleep(60);
	}

	free(device_id);

	return NULL;
}

void broker_on_connect(void)
{
	amqp_subscribe("temperature-limits");

	pthread_t temperature_thread;
	pthread_create(&temperature_thread, NULL, watch_temperature, NULL);
}

bool broker_on_message(char *exchange, char *message)
{
	cJSON *json = cJSON_Parse(message);
	if (cJSON_IsInvalid(json)) goto cancel;

	// Get values
	cJSON *reference_id = cJSON_GetObjectItem(json, "ReferenceId");
	if (!cJSON_IsString(reference_id)) goto cancel;

	cJSON *min_temp = cJSON_GetObjectItem(json, "TempLow");
	if (!cJSON_IsNumber(min_temp)) goto cancel;

	cJSON *max_temp = cJSON_GetObjectItem(json, "TempHigh");
	if (!cJSON_IsNumber(min_temp)) goto cancel;

	if (strcmp(reference_id->valuestring, get_device_id()) != 0) {
		goto cancel;
	}

	printf("Min temperature: %lf, Max temperature: %lf\n", min_temp->valuedouble, max_temp->valuedouble);

	set_min_temperature(min_temp->valuedouble);
	set_max_temperature(max_temp->valuedouble);

	cJSON_Delete(json);

	return true;

cancel:
	cJSON_Delete(json);

	return false;
}

int main(void)
{
	srand(time(NULL));

	init_amqp();

	return EXIT_SUCCESS;
}

