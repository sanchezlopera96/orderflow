# 4. Una base de datos PostgreSQL por servicio

- Estado: aceptada
- Fecha: 2026-07-29

## Contexto

Orders e Inventory son dueños de datos distintos. Compartir una base de datos acoplaría los
servicios y difuminaría la propiedad de los datos. Además, el pedido necesita validar que un SKU
exista en el catálogo sin llamar sincrónicamente a Inventory (lo que los acoplaría en tiempo de
ejecución).

## Decisión

Cada servicio tiene su propio `DbContext` de EF Core y su propia base de datos; ninguno lee las
tablas del otro. Orders mantiene una pequeña tabla de catálogo (read model) sembrada con los
mismos SKUs que Inventory, de modo que la validación de SKU es local.

## Consecuencias

- Propiedad clara y evolución independiente de cada esquema.
- La lista de SKUs se duplica en ambos seeds; se asume como el costo del desacoplamiento.
- Se usa PostgreSQL para ambos, manteniendo liviano el footprint del Compose.
