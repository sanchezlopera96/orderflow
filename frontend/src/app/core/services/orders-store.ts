import { computed, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, merge, of, Subject, switchMap, tap, timer } from 'rxjs';
import { Observable } from 'rxjs';
import { CreateOrderRequest, Order } from '../models/order.model';
import { OrderService } from './order.service';

/** Cada cuántos milisegundos se refresca la lista de pedidos. */
const POLL_INTERVAL_MS = 3000;

/**
 * Estado compartido de los pedidos, basado en signals. Hace polling periódico a la API y expone
 * la lista, el estado de carga y el de error. El formulario y la lista lo comparten sin necesidad
 * de una librería de estado.
 */
@Injectable({ providedIn: 'root' })
export class OrdersStore {
  private readonly orderService = inject(OrderService);

  private readonly _orders = signal<Order[]>([]);
  private readonly _loading = signal(true);
  private readonly _error = signal<string | null>(null);
  private readonly refresh$ = new Subject<void>();

  readonly orders = this._orders.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly isEmpty = computed(() => !this._loading() && this._orders().length === 0);

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
  }

  /** Fuerza un refresco inmediato, sin esperar al siguiente ciclo de polling. */
  refresh(): void {
    this.refresh$.next();
  }

  /** Crea un pedido y, al terminar bien, refresca la lista de inmediato. */
  createOrder(request: CreateOrderRequest): Observable<Order> {
    return this.orderService.createOrder(request).pipe(tap(() => this.refresh()));
  }
}
