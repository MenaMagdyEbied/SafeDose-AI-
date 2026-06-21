import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PatientService } from '../../core/services/patient';
import { FamilyPlan } from './family-plan';

describe('FamilyPlan', () => {
  let component: FamilyPlan;
  let fixture: ComponentFixture<FamilyPlan>;
  let patientService: jasmine.SpyObj<PatientService>;

  beforeEach(async () => {
    patientService = jasmine.createSpyObj('PatientService', [
      'getMyPatients',
      'createPatient',
      'updatePatient',
      'deletePatient',
    ]);

    patientService.getMyPatients.and.resolveTo([]);
    patientService.createPatient.and.resolveTo({} as never);
    patientService.updatePatient.and.resolveTo({} as never);
    patientService.deletePatient.and.resolveTo();

    await TestBed.configureTestingModule({
      imports: [FamilyPlan],
      providers: [{ provide: PatientService, useValue: patientService }],
    }).compileComponents();

    fixture = TestBed.createComponent(FamilyPlan);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should create a patient when gender is 0', async () => {
    component.openAddModal();
    component.form.patchValue({
      fullName: 'أحمد محمد',
      dateOfBirth: '2000-01-01',
      gender: 0,
      bloodType: 'O+',
      chronicConditions: 'سكري',
      allergies: 'حبوب',
    });

    await component.saveMember();

    expect(patientService.createPatient).toHaveBeenCalled();
    expect(component.showModal).toBeFalse();
  });
});
