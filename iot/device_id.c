#include <stdlib.h>
#include <stdio.h>

#include "device_id.h"

#define DEVICE_ID_SIZE 5

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

void destroy_device_id(char *device_id)
{
	free(device_id);
}

