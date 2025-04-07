#include <rabbitmq-c/amqp.h>
#include <rabbitmq-c/tcp_socket.h>
#include <stdbool.h>
#include <stdlib.h>
#include <string.h>
#include <stdio.h>

#include "../config.h"

amqp_connection_state_t conn;
amqp_socket_t *socket;
amqp_channel_t channel = 1;

void broker_on_connect(void);

bool broker_on_message(char *exchange, char *message);

void amqp_send_message(char *queue, char *message)
{
	amqp_basic_properties_t props;
	props._flags = AMQP_BASIC_CONTENT_TYPE_FLAG | AMQP_BASIC_DELIVERY_MODE_FLAG;
	props.content_type = amqp_literal_bytes("text/plain");
	props.delivery_mode = 2;

	amqp_basic_publish(conn, channel, amqp_cstring_bytes(queue), amqp_cstring_bytes(queue), 0, 0, &props, amqp_cstring_bytes(message));
}

void amqp_subscribe(char *queue)
{
	amqp_basic_consume(conn, channel, amqp_cstring_bytes(queue), amqp_cstring_bytes("iot"), 1, 0, 0, amqp_empty_table);
}

char *amqp_bytes_to_cstring(amqp_bytes_t bytes)
{
	char *str = malloc(bytes.len + 1);

	memcpy(str, bytes.bytes, bytes.len);
	str[bytes.len] = '\0';

	return str;
}

void init_amqp(void)
{
	conn = amqp_new_connection();

	socket = amqp_tcp_socket_new(conn);
	amqp_socket_open(socket, AMQP_IP, AMQP_PORT);

	amqp_login(conn, "/", 0, 131072, 0, AMQP_SASL_METHOD_PLAIN, AMQP_USER, AMQP_PASSWORD);

	amqp_channel_open(conn, channel);

	broker_on_connect();

	while (true) {
		amqp_envelope_t envelope;
		amqp_consume_message(conn, &envelope, NULL, 0);

		char *exchange_name = amqp_bytes_to_cstring(envelope.exchange);
		char *message = amqp_bytes_to_cstring(envelope.message.body);

		if (broker_on_message(exchange_name, message)) {
			amqp_basic_ack(conn, channel, envelope.delivery_tag, 0);
		}

		free(exchange_name);
		free(message);
		amqp_destroy_envelope(&envelope);
	}
}

