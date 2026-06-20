import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ScanPrescription } from './scan-prescription';

describe('ScanPrescription', () => {
  let component: ScanPrescription;
  let fixture: ComponentFixture<ScanPrescription>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ScanPrescription],
    }).compileComponents();

    fixture = TestBed.createComponent(ScanPrescription);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
