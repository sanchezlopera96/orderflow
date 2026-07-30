export const environment = {
  production: true,
  // Vacío = rutas relativas. En dev, el proxy reenvía /orders a la API;
  // en Docker, nginx hace lo mismo. Así el frontend no depende del host de la API.
  apiBaseUrl: '',
};
