void init_amqp(void);

void amqp_send_message(char *topic, char *message);

void amqp_subscribe(char *exchange, char *queue);

