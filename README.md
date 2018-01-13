# Partial Foods - Order Command Service
The Order Command service for the partial foods sample is responsible for accepting commands from clients. If the commands are validated and rules are applied successfully, then corresponding _events_ are emitted - in this case, on Kafka topics.

The Order Command Service is a gRPC service.

## Notes
Remember that to override settings from configuration, replace the `:` path character from code with TWO underscores, e.g. `SERVICE__PORT`