import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiProblem } from '../../../core/models/order.model';
import { OrdersStore } from '../../../core/services/orders-store';

/** Formulario para crear un pedido, con validaciones visibles y manejo de errores en pantalla. */
@Component({
  selector: 'app-order-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './order-form.component.html',
  styleUrl: './order-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrderFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly store = inject(OrdersStore);

  protected readonly submitting = signal(false);
  protected readonly serverError = signal<string | null>(null);
  protected readonly justCreated = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    customerName: ['', [Validators.required, Validators.maxLength(200)]],
    sku: ['', [Validators.required]],
    quantity: [1, [Validators.required, Validators.min(1), Validators.max(100)]],
  });

  protected submit(): void {
    this.serverError.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.store.createOrder(this.form.getRawValue()).subscribe({
      next: () => {
        this.submitting.set(false);
        this.form.reset({ customerName: '', sku: '', quantity: 1 });
        this.justCreated.set(true);
        setTimeout(() => this.justCreated.set(false), 2500);
      },
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        this.serverError.set(this.describeError(error));
      },
    });
  }

  protected showError(control: string, error: string): boolean {
    const field = this.form.get(control);
    return !!field && field.touched && field.hasError(error);
  }

  /** Traduce un error HTTP en un mensaje legible para el usuario. */
  private describeError(error: HttpErrorResponse): string {
    const problem = error.error as ApiProblem | undefined;

    if (problem?.title === 'order.sku_not_found') {
      return 'El SKU indicado no existe en el catálogo.';
    }
    if (problem?.errors) {
      return Object.values(problem.errors).flat().join(' ');
    }
    if (problem?.detail) {
      return problem.detail;
    }
    if (error.status === 0) {
      return 'No se pudo contactar el servidor.';
    }
    return 'No se pudo crear el pedido. Intenta de nuevo.';
  }
}
