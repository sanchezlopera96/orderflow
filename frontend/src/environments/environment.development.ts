// Para desarrollo. Si se quisiera apuntar directo a la API local sin el proxy, bastaría con
// cambiar apiBaseUrl por 'http://localhost:5080' (y habilitar CORS en la API).
const apiBaseUrl = '';

export const environment = {
  production: false,
  apiBaseUrl,
  api: {
    orders: `${apiBaseUrl}/orders`,
    order: (id: string) => `${apiBaseUrl}/orders/${id}`,
    products: `${apiBaseUrl}/products`,
    hub: `${apiBaseUrl}/hubs/orders`,
  },
};
