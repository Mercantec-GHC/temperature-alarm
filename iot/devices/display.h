typedef int display_handle_t;

display_handle_t init_display();

void display_set_cursor_pos(display_handle_t display, int x, int y);

void display_write_str(display_handle_t display, char *str);

void display_write_char(display_handle_t display, char ch);

