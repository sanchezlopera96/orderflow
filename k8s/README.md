# Despliegue en Kubernetes

Manifiestos para desplegar OrderFlow completo (los dos servicios, el broker y las dos bases).
No se necesita un clúster para evaluar el diseño; si quieres probarlo, sirve cualquier clúster
local (minikube, kind, Docker Desktop con Kubernetes).

## Construir y cargar las imágenes

Los manifiestos usan `imagePullPolicy: IfNotPresent` con imágenes locales (sin registro):

```bash
docker build -t orderflow-orders-api:latest       -f src/Orders/Orders.Api/Dockerfile .
docker build -t orderflow-inventory-worker:latest -f src/Inventory/Inventory.Worker/Dockerfile .
docker build -t orderflow-frontend:latest         ./frontend

# kind:      kind load docker-image orderflow-orders-api:latest orderflow-inventory-worker:latest orderflow-frontend:latest
# minikube:  minikube image load orderflow-orders-api:latest  (repetir por imagen)
```

## Aplicar

```bash
kubectl apply -f k8s/
```

El orden de los archivos (por prefijo numérico) crea primero namespace, config y datos, y luego los
servicios. Los `readinessProbe` evitan que reciban tráfico antes de estar listos.

## Acceder al panel

```bash
kubectl -n orderflow port-forward svc/frontend 4200:80
# http://localhost:4200
```

## Notas de diseño

- **Config**: no sensible en un `ConfigMap`; contraseñas y connection strings en un `Secret`
  (en un entorno real, gestionado con Sealed Secrets/Vault, no versionado).
- **Probes**: la Orders API y el frontend usan `httpGet` (la API contra el `/health` real, que
  chequea PostgreSQL y RabbitMQ). El worker no expone HTTP; al ser idempotente y reintentar, es
  seguro reiniciarlo, por eso no lleva probes HTTP.
- **Persistencia**: cada base tiene su `PersistentVolumeClaim`.
- **Escalado**: la Orders API y el worker pueden escalar horizontalmente (más réplicas) — la
  idempotencia (inbox por `EventId`) y la concurrencia optimista lo hacen seguro. En un siguiente
  paso se añadiría un `HorizontalPodAutoscaler`.
