import pika
import json

connectionParameters = pika.ConnectionParameters(host='localhost')
connection = pika.BlockingConnection(connectionParameters)
channel = connection.channel()

exchangeName = 'EmailExchange'
queueName = 'EmailQueue'
routingKey = ''

channel.exchange_declare(exchange=exchangeName)

message = {"token": "token",
           "serviceName": "ServiceName",
           "message": {
               "templateName": "typeMessage",
               "from": "SenderUserId",
               "to": ["ReceiverUserIds"]
           }}

channel.basic_publish(exchange=exchangeName,
                      routing_key=routingKey,
                      body=json.dumps(message))

print(" [x] Sent %r" % message)
connection.close()
