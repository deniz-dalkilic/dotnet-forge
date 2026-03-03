# Messaging

RabbitMQ is used for integration events with explicit code paths (no mediator/event framework magic).

## Components

- **Producer**: API endpoint writes to the database, then publishes via `IEventBus`.
- **Bus abstraction**: `IEventBus.PublishAsync<T>(T evt, CancellationToken ct)` keeps Application layer broker-agnostic.
- **RabbitMQ implementation**: `RabbitMqEventBus` serializes JSON and publishes to a topic exchange.
- **Consumer**: Worker hosted service (`SampleItemCreatedConsumerService`) declares exchange/queue/binding and consumes `SampleItemCreated`.

## Naming

- **Exchange**: `template.events` (configurable via `RabbitMq:Exchange`).
- **Routing key**: event CLR type name (example: `SampleItemCreated`).
- **Queue**: explicit worker queue name (`sample-item-created.worker`).

This naming is deliberate and traceable: route and queue intent are visible in code and logs.

## Reliability and retries

- Publisher uses retry with exponential backoff for connection/publish failures.
- Consumer runs in a reconnect loop with bounded exponential backoff.
- Both sides emit attempt counts and delay values in logs for operational debugging.

## Idempotency guidance

RabbitMQ delivery is at-least-once. Consumers must be idempotent:

- Use an idempotency key (typically event ID) and persist processing state.
- Make handlers safe to run multiple times.
- Acknowledge only after successful processing.
- On transient failures, avoid ack so message can be retried.

Current sample consumer logs and acknowledges to demonstrate wiring. Real handlers should add durable idempotency checks before side effects.
