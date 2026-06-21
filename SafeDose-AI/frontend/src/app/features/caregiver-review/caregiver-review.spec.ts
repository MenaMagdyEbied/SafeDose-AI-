import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CaregiverReview } from './caregiver-review';

describe('CaregiverReview', () => {
  let component: CaregiverReview;
  let fixture: ComponentFixture<CaregiverReview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CaregiverReview],
    }).compileComponents();

    fixture = TestBed.createComponent(CaregiverReview);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
