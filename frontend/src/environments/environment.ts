// URL base de la API. Se deja relativa: en desarrollo el proxy reenvía a la API local y en Docker
// nginx hace lo mismo, así que el mismo build funciona en ambos sin depender del host ni de CORS.
const apiBaseUrl = '';

export const environment = {
  production: true,
  apiBaseUrl,
  // Rutas de la API centralizadas. Las que llevan parámetro son funciones.
  api: {
    orders: `${apiBaseUrl}/orders`,
    order: (id: string) => `${apiBaseUrl}/orders/${id}`,
    products: `${apiBaseUrl}/products`,
    hub: `${apiBaseUrl}/hubs/orders`,
  },
};
