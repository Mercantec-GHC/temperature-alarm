#include <rabbitmq-c/amqp.h>
#include <rabbitmq-c/tcp_socket.h>

#include "../config.h"

amqp_connection_state_t conn;
amqp_socket_t *socket;

void broker_on_connect(void);

void amqp_send_message(char *queue, char *message)
{
	amqp_basic_properties_t props;
	props._flags = AMQP_BASIC_CONTENT_TYPE_FLAG | AMQP_BASIC_DELIVERY_MODE_FLAG;
	props.content_type = amqp_literal_bytes("text/plain");
	props.delivery_mode = 2;

	amqp_basic_publish(conn, 1, amqp_cstring_bytes(queue), amqp_cstring_bytes(queue), 0, 0, &props, amqp_cstring_bytes(message));
}

void init_amqp(void)
{
	conn = amqp_new_connection();

	socket = amqp_tcp_socket_new(conn);
	amqp_socket_open(socket, AMQP_IP, AMQP_PORT);

	amqp_login(conn, "/", 0, 131072, 0, AMQP_SASL_METHOD_PLAIN, AMQP_USER, AMQP_PASSWORD);

	amqp_channel_open(conn, 1);

	broker_on_connect();

	for (;;);
}
