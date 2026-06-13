import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CaregiverDashboard } from './caregiver-dashboard';

describe('CaregiverDashboard', () => {
  let component: CaregiverDashboard;
  let fixture: ComponentFixture<CaregiverDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CaregiverDashboard],
    }).compileComponents();

    fixture = TestBed.createComponent(CaregiverDashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
