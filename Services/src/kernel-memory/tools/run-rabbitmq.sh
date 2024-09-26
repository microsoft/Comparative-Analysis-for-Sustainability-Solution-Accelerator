# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT License.

docker run -it --rm --name rabbitmq \
  -e RABBITMQ_DEFAULT_USER=user \
  -e RABBITMQ_DEFAULT_PASS=password \
  -p 5672:5672 \
  rabbitmq:3
