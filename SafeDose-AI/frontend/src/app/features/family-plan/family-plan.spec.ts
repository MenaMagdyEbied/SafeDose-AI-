import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PatientService } from '../../core/services/patient';
import { FamilyPlan } from './family-plan';

describe('FamilyPlan', () => {
  let component: FamilyPlan;
  let fixture: ComponentFixture<FamilyPlan>;
  let patientService: any;
  let createPatientCalls = 0;
  let setRunningPatientId: number | null = null;

  beforeEach(async () => {
    createPatientCalls = 0;
    setRunningPatientId = null;

    patientService = {
      getMyPatients: async () => [],
      createPatient: async () => {
        createPatientCalls += 1;
        return {};
      },
      updatePatient: async () => ({}),
      deletePatient: async () => undefined,
      setRunningPatient: async (id: number) => {
        setRunningPatientId = id;
      },
      resolvePatientId: (patient: any) => patient?.patientId ?? patient?.id ?? null,
      currentPatientId: null,
    };

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

    expect(createPatientCalls).toBe(1);
    expect(component.showModal()).toBeFalsy();
  });

  it('should activate the selected member through the running patient endpoint', () => {
    const member = {
      id: 7,
      patientId: 7,
      fullName: 'مريض ١',
      dateOfBirth: '1990-01-01',
      bloodType: 'O+',
      chronicConditions: [],
      allergies: [],
    } as any;

    component.activateMember(member);

    expect(setRunningPatientId).toBe(7);
  });
});
