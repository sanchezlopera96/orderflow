# 2. Clean Architecture "lite": tres proyectos por servicio

- Estado: aceptada
- Fecha: 2026-07-29

## Contexto

El problema es pequeño (tres endpoints, un worker, un puñado de entidades). Un split completo de
Domain/Application/Infrastructure/Api produciría ocho proyectos por servicio y se leería como
sobreingeniería. Un único proyecto con carpetas, por el contrario, no deja que el compilador
imponga la dirección de las dependencias.

## Decisión

Tres proyectos por servicio: `Domain` (sin dependencias), `Infrastructure` (EF Core, mensajería) y
el host (`Api` o `Worker`, que actúa como composition root). Las dependencias apuntan hacia
adentro, hacia el dominio. Los contratos transversales viven en un proyecto compartido
`BuildingBlocks`.

## Consecuencias

- La regla de dependencias la imponen las project references, no la convención.
- Sin CQRS/MediatR: las operaciones son triviales y un mediator agregaría indirección sin valor
  (YAGNI).
- Si el dominio creciera, se podría introducir una capa `Application` sin reestructurar.
