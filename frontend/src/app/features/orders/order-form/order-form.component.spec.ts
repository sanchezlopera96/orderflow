import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormGroup } from '@angular/forms';
import { OrderFormComponent } from './order-form.component';

describe('OrderFormComponent', () => {
  let fixture: ComponentFixture<OrderFormComponent>;
  let component: OrderFormComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrderFormComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(OrderFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  const form = () => (component as unknown as { form: FormGroup }).form;

  it('starts invalid with an empty customer name', () => {
    expect(form().invalid).toBeTrue();
  });

  it('becomes valid with a well-formed order', () => {
    form().setValue({ customerName: 'Ada Lovelace', sku: 'ABC-01', quantity: 2 });
    expect(form().valid).toBeTrue();
  });

  it('rejects a quantity above 100', () => {
    form().setValue({ customerName: 'Ada Lovelace', sku: 'ABC-01', quantity: 101 });
    expect(form().get('quantity')?.valid).toBeFalse();
  });
});
