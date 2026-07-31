import { computed, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, merge, Observable, of, Subject, switchMap, tap, timer } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { API_ROUTES } from '../config/api.config';
import { CreateOrderRequest, Order, Product } from '../models/order.model';
import { OrderService } from './order.service';

/**
 * Cada cuántos milisegundos se refresca la lista como red de seguridad. El tiempo real llega por
 * SignalR (push instantáneo); este polling lento solo cubre el caso de que el socket se caiga.
 */
const POLL_INTERVAL_MS = 10000;

/**
 * Estado compartido de los pedidos, basado en signals. Recibe los cambios en tiempo real por
 * SignalR y los aplica sobre la lista; además hace una carga inicial y un polling de respaldo.
 */
@Injectable({ providedIn: 'root' })
export class OrdersStore {
  private readonly orderService = inject(OrderService);
  private readonly hubUrl = inject(API_ROUTES).hub;

  private readonly _orders = signal<Order[]>([]);
  private readonly _loading = signal(true);
  private readonly _error = signal<string | null>(null);
  private readonly _products = signal<Product[]>([]);
  private readonly refresh$ = new Subject<void>();

  readonly orders = this._orders.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly isEmpty = computed(() => !this._loading() && this._orders().length === 0);
  readonly products = this._products.asReadonly();

  constructor() {
    merge(timer(0, POLL_INTERVAL_MS), this.refresh$)
      .pipe(
        switchMap(() =>
          this.orderService.getOrders().pipe(
            catchError(() => {
              this._error.set('No se pudieron cargar los pedidos.');
              return of(null);
            }),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((orders) => {
        if (orders) {
          this._orders.set(orders);
          this._error.set(null);
        }
        this._loading.set(false);
      });

    this.connectRealtime();
    this.loadProducts();
  }

  refresh(): void {
    this.refresh$.next();
  }

  createOrder(request: CreateOrderRequest): Observable<Order> {
    return this.orderService.createOrder(request).pipe(tap((order) => this.upsert(order)));
  }

  private connectRealtime(): void {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl)
      .withAutomaticReconnect()
      .build();

    connection.on('OrderChanged', (order: Order) => this.upsert(order));
    // Al reconectar tras una caída, se recarga todo por si se perdió algún evento.
    connection.onreconnected(() => this.refresh());

    connection.start().catch(() => {
      // Si SignalR no puede conectar, el polling de respaldo mantiene la lista al día.
    });
  }

  /** Carga el catálogo una vez, para poblar las sugerencias de SKU del formulario. */
  private loadProducts(): void {
    this.orderService
      .getProducts()
      .pipe(
        catchError(() => of([] as Product[])),
        takeUntilDestroyed(),
      )
      .subscribe((products) => this._products.set(products));
  }

  /** Inserta o reemplaza un pedido por id, manteniendo el orden por fecha de creación descendente. */
  private upsert(order: Order): void {
    const others = this._orders().filter((existing) => existing.id !== order.id);
    this._orders.set([order, ...others].sort((a, b) => b.createdAt.localeCompare(a.createdAt)));
    this._loading.set(false);
  }
}
