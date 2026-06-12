import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CaregiverResults } from './caregiver-results';

describe('CaregiverResults', () => {
  let component: CaregiverResults;
  let fixture: ComponentFixture<CaregiverResults>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CaregiverResults],
    }).compileComponents();

    fixture = TestBed.createComponent(CaregiverResults);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
