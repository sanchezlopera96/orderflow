import { ChangeDetectionStrategy, Component } from '@angular/core';
import { OrdersPageComponent } from './features/orders/orders-page/orders-page.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [OrdersPageComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {}
