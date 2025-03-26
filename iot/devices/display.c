#include <linux/i2c-dev.h>
#include <stdlib.h>
#include <stdio.h>
#include <fcntl.h>
#include <string.h>
#include <errno.h>
#include <sys/ioctl.h>
#include <i2c/smbus.h>

#include "display.h"

#define JHD1313_BUS "/dev/i2c-2"
#define JHD1313_ADR 0x3e

void display_set_cursor_pos(display_handle_t display, int x, int y)
{
	i2c_smbus_write_byte_data(display, 0x00, (y ? 0xC0 : 0x80) + x);
}

void display_write_str(display_handle_t display, char *str)
{
	while (*str) {
		display_write_char(display, *str);
		str++;
	}
}

void display_write_char(display_handle_t display, char ch)
{
	i2c_smbus_write_byte_data(display, 0x40, ch);
}

display_handle_t init_display()
{
	int file = open(JHD1313_BUS, O_RDWR);

	if (file < 0) {
		perror("Error opening display device");
		exit(EXIT_FAILURE);
	}

	if (ioctl(file, I2C_SLAVE, JHD1313_ADR) == -1) {
		fprintf(stderr, "ERROR: setting address %d on i2c bus %s with ioctl() - %s", JHD1313_ADR, JHD1313_BUS, strerror(errno));
		exit(EXIT_FAILURE);
	}

	// 2 line mode, 5x8
	i2c_smbus_write_byte_data(file, 0x00, 0x28);
	// Display on, cursor on, blink off
	i2c_smbus_write_byte_data(file, 0x00, 0x0C);
	// Display clear
	i2c_smbus_write_byte_data(file, 0x00, 0x01);
	// Entry mode set
	i2c_smbus_write_byte_data(file, 0x00, 0x06);

	i2c_smbus_write_byte_data(file, 0x00, 0x02);

	return file;
}

