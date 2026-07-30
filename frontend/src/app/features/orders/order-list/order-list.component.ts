import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { OrderStatus } from '../../../core/models/order.model';
import { OrdersStore } from '../../../core/services/orders-store';

/** Lista de pedidos con su estado, refrescada por polling desde el store. */
@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrderListComponent {
  protected readonly store = inject(OrdersStore);

  private static readonly labels: Record<OrderStatus, string> = {
    Pending: 'Pendiente',
    Confirmed: 'Confirmado',
    Rejected: 'Rechazado',
  };

  protected statusLabel(status: OrderStatus): string {
    return OrderListComponent.labels[status];
  }
}
