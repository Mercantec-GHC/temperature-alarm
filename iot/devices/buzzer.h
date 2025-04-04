#include <gpiod.h>

typedef struct {
	struct gpiod_chip *chip;
	struct gpiod_line *line;
} buzzer_handle_t;

buzzer_handle_t init_buzzer();

void sound_buzzer(buzzer_handle_t buzzer, int delay, int length);

void destroy_buzzer(buzzer_handle_t buzzer);

