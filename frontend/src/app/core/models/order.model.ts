export type OrderStatus = 'Pending' | 'Confirmed' | 'Rejected';

export interface Order {
  id: string;
  customerName: string;
  sku: string;
  quantity: number;
  status: OrderStatus;
  createdAt: string;
}

export interface CreateOrderRequest {
  customerName: string;
  sku: string;
  quantity: number;
}

/** Cuerpo de un error de la API (ProblemDetails / ValidationProblem). */
export interface ApiProblem {
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
