import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShoppingCart } from './shopping-cart';

describe('ShoppingCart', () => {
  let component: ShoppingCart;
  let fixture: ComponentFixture<ShoppingCart>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ShoppingCart],
    }).compileComponents();

    fixture = TestBed.createComponent(ShoppingCart);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
