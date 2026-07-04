import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateAnAccount } from './create-an-account';

describe('CreateAnAccount', () => {
  let component: CreateAnAccount;
  let fixture: ComponentFixture<CreateAnAccount>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CreateAnAccount],
    }).compileComponents();

    fixture = TestBed.createComponent(CreateAnAccount);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
