import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PatientHome } from './patient-home';

describe('PatientHome', () => {
  let component: PatientHome;
  let fixture: ComponentFixture<PatientHome>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientHome],
    }).compileComponents();

    fixture = TestBed.createComponent(PatientHome);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
