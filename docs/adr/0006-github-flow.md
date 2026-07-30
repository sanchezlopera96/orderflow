# 6. GitHub Flow en vez de Git Flow completo

- Estado: aceptada
- Fecha: 2026-07-29

## Contexto

El ejercicio valora un historial de commits real. Git Flow completo (ramas develop/release/hotfix)
es pesado para un ejercicio de dos días y un solo autor.

## Decisión

Usar GitHub Flow: `main` siempre es liberable; cada etapa se construye sobre una rama de feature de
vida corta (`feat/…`, `chore/…`, `docs/…`) y se integra mediante un pull request. Los mensajes de
commit siguen Conventional Commits.

## Consecuencias

- El historial se lee como una secuencia de incrementos revisables y autocontenidos.
- Menos ceremonia que Git Flow, acorde al alcance.
