import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminPricing } from './admin-pricing';

describe('AdminPricing', () => {
  let component: AdminPricing;
  let fixture: ComponentFixture<AdminPricing>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminPricing],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminPricing);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should include the free, monthly, and annual plans', () => {
    const planIds = component.plans.map((plan) => plan.id);

    expect(planIds).toEqual(['free', 'premium-monthly', 'premium-annual']);
  });
});
