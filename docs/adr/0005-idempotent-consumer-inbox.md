# 5. Consumidor idempotente mediante el inbox pattern

- Estado: aceptada
- Fecha: 2026-07-29

## Contexto

La entrega es at-least-once: el mismo evento `OrderCreated` puede llegar más de una vez. El stock
no debe descontarse dos veces. También existe el problema de dual-write al publicar después de un
commit en base de datos.

## Decisión

El consumer de Inventory mantiene una tabla `ProcessedEvents` indexada por `EventId`. Dentro de una
única transacción verifica si el evento ya fue procesado; si lo fue, hace ack y se detiene; si no,
descuenta stock, registra el `EventId` y hace commit. Las filas de stock usan concurrencia
optimista de PostgreSQL (`xmin`), de modo que mensajes concurrentes para el mismo SKU no pueden
descontar de más.

Del lado de Orders, la idempotencia también proviene de la máquina de estados: un resultado de
stock solo se aplica mientras el pedido está en `Pending`, así que un resultado duplicado es un
no-op.

## Consecuencias

- Las entregas duplicadas son demostrablemente seguras (cubiertas por pruebas).
- El dual-write al publicar se maneja de forma pragmática (recuperación de conexión) por ahora; un
  transactional outbox es la evolución documentada.
