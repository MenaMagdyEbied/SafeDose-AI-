import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FamilyPlan } from './family-plan';

describe('FamilyPlan', () => {
  let component: FamilyPlan;
  let fixture: ComponentFixture<FamilyPlan>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FamilyPlan],
    }).compileComponents();

    fixture = TestBed.createComponent(FamilyPlan);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
