import { ChangeDetectionStrategy, Component } from '@angular/core';
import { OrderFormComponent } from '../order-form/order-form.component';
import { OrderListComponent } from '../order-list/order-list.component';

@Component({
  selector: 'app-orders-page',
  standalone: true,
  imports: [OrderFormComponent, OrderListComponent],
  templateUrl: './orders-page.component.html',
  styleUrl: './orders-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrdersPageComponent {}
