import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { OrderService } from './order.service';

describe('OrderService', () => {
  let service: OrderService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OrderService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(OrderService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('requests the order list with GET /orders', () => {
    service.getOrders().subscribe();

    const req = httpMock.expectOne('/orders');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('creates an order with POST /orders and the given body', () => {
    const request = { customerName: 'Ada Lovelace', sku: 'ABC-01', quantity: 2 };

    service.createOrder(request).subscribe();

    const req = httpMock.expectOne('/orders');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush({ id: '1', ...request, status: 'Pending', createdAt: '2026-01-01T00:00:00Z' });
  });
});
