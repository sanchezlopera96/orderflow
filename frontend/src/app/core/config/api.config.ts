import { InjectionToken } from '@angular/core';
import { environment } from '../../../environments/environment';

/** Rutas de la API, tomadas del ambiente. Se inyectan para no armar URLs en los servicios. */
export type ApiRoutes = typeof environment.api;

export const API_ROUTES = new InjectionToken<ApiRoutes>('API_ROUTES', {
  providedIn: 'root',
  factory: () => environment.api,
});
