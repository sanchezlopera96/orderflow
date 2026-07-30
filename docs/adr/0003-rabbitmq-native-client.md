# 3. RabbitMQ con el cliente nativo en vez de MassTransit

- Estado: aceptada
- Fecha: 2026-07-29

## Contexto

Se exige comunicación asíncrona sobre un broker real. MassTransit es la opción idiomática en .NET
y ofrece reintentos, serialización y helpers de idempotencia de fábrica. Sin embargo, el ejercicio
pide ver la idempotencia y el manejo de fallos implementados, y advierte contra frameworks que
resuelvan el problema por uno.

## Decisión

Usar `RabbitMQ.Client` directamente detrás de una fina abstracción `IEventPublisher` y un consumer
explícito. La publicación/consumo, los acknowledgements y la verificación de idempotencia se
escriben a mano.

## Consecuencias

- El comportamiento de la mensajería queda totalmente visible y explicable.
- Más boilerplate que con MassTransit; se asume a conciencia para este ejercicio.
- La abstracción mantiene el transporte intercambiable y fácil de faker en pruebas.
