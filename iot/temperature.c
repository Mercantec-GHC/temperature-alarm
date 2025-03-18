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

#define MPC9808_BUS	"/dev/i2c-2"
#define MPC9808_ADR	0x18

#define CONFIG_REG 0x01
#define TUPPER_REG 0x02
#define TLOWER_REG 0x03
#define TCRIT_REG  0x04
#define TA_REG     0x05
#define MANID_REG  0x06
#define DEVID_REG  0x07
#define RES_REG    0x08

int file;

uint8_t get_byte_in_integer(int num, int n)
{
	return (num >> (8*n)) & 0xff;
}

double get_temperature()
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

void init_temperature(void)
{
	file = open(MPC9808_BUS, O_RDWR);
	if (file < 0) {
		fprintf(stderr, "Error opening temperature sensor device (%s): %s\n", MPC9808_BUS, strerror(errno));
		exit(1);
	}

	if (ioctl(file, I2C_SLAVE, MPC9808_ADR) == -1) {
		fprintf(stderr, "ERROR: setting  address %d on i2c bus %s with ioctl() - %s", MPC9808_ADR, MPC9808_BUS, strerror(errno));
		exit(1);
	}

	int32_t reg32;
	uint16_t * const reg16poi = (uint16_t *) &reg32;
	uint8_t  * const reg8poi  = (uint8_t *)  &reg32;

	// Read manufactorer ID
	reg32 = i2c_smbus_read_word_data(file, MANID_REG);

	if ( reg32 < 0 ) {
			fprintf(stderr, "ERROR: Read failed  on i2c bus register %d - %s\n",  MANID_REG,strerror(errno));
			exit(1);
	}
	if (bswap_16(reg16poi[0]) != 0x0054) {
			fprintf(stderr, "Manufactorer ID wrong is 0x%x should be 0x54\n",__bswap_16(reg16poi[0]));
			exit(1);
	}

	// Read device ID and revision
	reg32 = i2c_smbus_read_word_data(file, DEVID_REG);
	if (reg32 < 0) {
			fprintf(stderr, "ERROR: Read failed  on i2c bus register %d - %s\n",  DEVID_REG,strerror(errno) );
			exit(1);
	}
	if (reg8poi[0] != 0x04) {
			fprintf(stderr, "Manufactorer ID OK but device ID wrong is 0x%x should be 0x4\n",reg8poi[0]);
			exit(1);
	}
}

