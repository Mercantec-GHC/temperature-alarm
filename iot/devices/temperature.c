#include <linux/i2c-dev.h>
#include <i2c/smbus.h>
#include <stdint.h>
#include <fcntl.h>
#include <byteswap.h>
#include <stdio.h>
#include <stdlib.h>
#include <errno.h>
#include <string.h>
#include <sys/ioctl.h>

#include "temperature.h"

#define MCP9808_BUS	"/dev/i2c-2"
#define MCP9808_ADR	0x18
#define MCP9808_MANID 0x0054
#define MCP9808_DEVID 0x04

#define CONFIG_REG 0x01
#define TUPPER_REG 0x02
#define TLOWER_REG 0x03
#define TCRIT_REG  0x04
#define TA_REG     0x05
#define MANID_REG  0x06
#define DEVID_REG  0x07
#define RES_REG    0x08

uint8_t get_byte_in_integer(int num, int n)
{
	return (num >> (8*n)) & 0xff;
}

double get_temperature(temperature_handle_t file)
{
	double temperature;
	int32_t reg32;
	uint8_t temperatureUpper;
	uint8_t temperatureLower;

	reg32 = i2c_smbus_read_word_data(file, TA_REG);
	reg32 = bswap_16(reg32); // swap to little endian
	reg32 = reg32 & 0x1FFF; // clear unused flags
	temperatureUpper = get_byte_in_integer(reg32, 1);
	temperatureLower = get_byte_in_integer(reg32, 0);

	if ((temperatureUpper & 0x10) == 0x10) {
			temperatureUpper = temperatureUpper & 0x0F; // clear negative flag
			temperature = 256.0 - (temperatureUpper * 16.0 + temperatureLower / 16.0);
	} else {
			temperature = (temperatureUpper * 16.0 + temperatureLower / 16.0);
	}

	return temperature;
}

temperature_handle_t init_temperature(void)
{
	int file = open(MCP9808_BUS, O_RDWR);

	if (file < 0) {
		perror("Error opening temperature sensor device");
		exit(EXIT_FAILURE);
	}

	if (ioctl(file, I2C_SLAVE, MCP9808_ADR) == -1) {
		fprintf(stderr, "ERROR: setting address %d on i2c bus %s with ioctl() - %s", MCP9808_ADR, MCP9808_BUS, strerror(errno));
		exit(EXIT_FAILURE);
	}

	int32_t reg32;
	uint16_t * const reg16poi = (uint16_t *) &reg32;
	uint8_t  * const reg8poi  = (uint8_t *)  &reg32;

	// Read manufactorer ID
	reg32 = i2c_smbus_read_word_data(file, MANID_REG);

	if (reg32 < 0) {
		fprintf(stderr, "Read failed on i2c bus register %d: %s\n", MANID_REG, strerror(errno));
		exit(EXIT_FAILURE);
	}
	if (bswap_16(reg16poi[0]) != MCP9808_MANID) {
		fprintf(stderr, "Invalid manufacturer ID: Expected 0x%x, got 0x%x\n", MCP9808_MANID, __bswap_16(reg16poi[0]));
		exit(EXIT_FAILURE);
	}

	// Read device ID and revision
	reg32 = i2c_smbus_read_word_data(file, DEVID_REG);
	if (reg32 < 0) {
		fprintf(stderr, "Read failed on i2c bus register %d - %s\n", DEVID_REG, strerror(errno));
		exit(EXIT_FAILURE);
	}
	if (reg8poi[0] != MCP9808_DEVID) {
		fprintf(stderr, "Invalid device ID - expected 0x%x, got 0x%x\n", MCP9808_DEVID, reg8poi[0]);
		exit(EXIT_FAILURE);
	}

	return file;
}

