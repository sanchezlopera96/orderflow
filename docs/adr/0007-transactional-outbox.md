# 7. Outbox transaccional para publicar de forma confiable

Fecha: 2026-07-31

## Estado

Aceptada.

## Contexto

Al crear un pedido, la Orders API hacía dos cosas: persistir el pedido en PostgreSQL y publicar el
evento `OrderCreated` en RabbitMQ. Son dos sistemas distintos, así que no hay una transacción que
los abarque a ambos (dual-write). Si el proceso o el broker fallaban entre el commit del pedido y la
publicación, el pedido quedaba en `Pending` y el evento se perdía: Inventory nunca se enteraba.

La versión previa lo manejaba de forma pragmática (publicar best-effort y registrar el fallo), pero
el evento podía perderse.

## Decisión

Se implementa un **outbox transaccional** en la Orders API:

- Al crear un pedido, el evento se guarda como una fila en la tabla `outbox_messages` **en la misma
  transacción** que el pedido (un solo `SaveChanges`). O se guardan ambos, o ninguno.
- Un `BackgroundService` (`OutboxDispatcher`) lee periódicamente los mensajes pendientes, los publica
  en RabbitMQ y los marca como procesados. Si la publicación falla, registra el error e incrementa
  los intentos, y el mensaje se reintenta en el siguiente ciclo.

Así, una vez que el pedido existe, su evento existe y terminará publicándose, aunque el broker esté
caído en ese momento.

## Consecuencias

- El evento nunca se pierde: la publicación pasa de "at-most-once best-effort" a "at-least-once".
- Se refuerza la idempotencia del consumidor (ADR 0005): como el outbox puede publicar un mensaje más
  de una vez (por ejemplo, si el proceso muere tras publicar pero antes de marcar), el inbox por
  `EventId` de Inventory garantiza que no se procese dos veces.
- Hay una pequeña latencia (el intervalo de sondeo del despachador) entre crear el pedido y publicar
  el evento. Es aceptable para este flujo.
- Un siguiente paso sería un despachador basado en notificaciones (LISTEN/NOTIFY) en vez de sondeo,
  y limpieza periódica de los mensajes ya procesados.
