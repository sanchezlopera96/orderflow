import { InjectionToken } from '@angular/core';
import { environment } from '../../../environments/environment';

/** URL base de la API. Se inyecta para poder cambiarla sin tocar los servicios. */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  providedIn: 'root',
  factory: () => environment.apiBaseUrl,
});
