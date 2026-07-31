import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_ROUTES } from '../config/api.config';
import { CreateOrderRequest, Order, Product } from '../models/order.model';

/** Acceso HTTP a los endpoints de pedidos y catálogo. */
@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(API_ROUTES);

  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.api.orders);
  }

  getOrder(id: string): Observable<Order> {
    return this.http.get<Order>(this.api.order(id));
  }

  createOrder(request: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.api.orders, request);
  }

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(this.api.products);
  }
}
