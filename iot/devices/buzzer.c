#include <gpiod.h>
#include <stdio.h>
#include <unistd.h>

#include "buzzer.h"

buzzer_handle_t init_buzzer()
{
	struct gpiod_chip *chip = gpiod_chip_open_by_name("gpiochip1");
	if (chip == NULL) {
		fprintf(stderr, "Unable to find gpiochip1\n");
		exit(EXIT_FAILURE);
	}

	struct gpiod_line *line = gpiod_chip_get_line(chip, 19);
	if (line == NULL) {
		fprintf(stderr, "Unable to get line 19\n");
		exit(EXIT_FAILURE);
	}

	gpiod_line_request_output(line, "buzzer", 0);

	return (buzzer_handle_t) {
		chip,
		line,
	};
}

void sound_buzzer(buzzer_handle_t buzzer, int delay, int length)
{
	for (int cycle = 0; cycle < length; cycle++) {
		gpiod_line_set_value(buzzer.line, 1);
		usleep(delay);
		gpiod_line_set_value(buzzer.line, 0);
		usleep(delay);
	}
}

void destroy_buzzer(buzzer_handle_t buzzer)
{
	gpiod_line_release(buzzer.line);
	gpiod_chip_close(buzzer.chip);
}

