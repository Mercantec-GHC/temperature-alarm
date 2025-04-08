#include <stdlib.h>
#include <stdio.h>
#include <math.h>

#include "device_properties.h"

char *get_device_id(void)
{
	FILE *file = fopen("device_id.txt", "r");

	if (!file) {
		char *device_id = generate_device_id();

		file = fopen("device_id.txt", "w");
		fprintf(file, "%s", device_id);
		fclose(file);

		return device_id;
	}

	char *device_id = malloc(DEVICE_ID_SIZE + 1);
	fgets(device_id, DEVICE_ID_SIZE + 1, file);
	fclose(file);

	return device_id;
}

char *generate_device_id(void)
{
	char *device_id = malloc(DEVICE_ID_SIZE + 1);

	for (int i = 0; i < DEVICE_ID_SIZE; i++) {
		device_id[i] = 'A' + (random() % 26);
	}

	device_id[DEVICE_ID_SIZE] = '\0';

	return device_id;
}

void set_min_temperature(double temperature)
{
	FILE *file = fopen("min_temp.txt", "w");
	fprintf(file, "%lf", temperature);
	fclose(file);
}

double get_min_temperature(void)
{
	FILE *file = fopen("min_temp.txt", "r");
	if (!file) return NAN;

	double temperature = NAN;
	fscanf(file, "%lf", &temperature);
	fclose(file);

	return temperature;
}

void set_max_temperature(double temperature)
{
	FILE *file = fopen("max_temp.txt", "w");
	fprintf(file, "%lf", temperature);
	fclose(file);
}

double get_max_temperature(void)
{
	FILE *file = fopen("max_temp.txt", "r");
	if (!file) return NAN;

	double temperature = NAN;
	fscanf(file, "%lf", &temperature);
	fclose(file);

	return temperature;
}

