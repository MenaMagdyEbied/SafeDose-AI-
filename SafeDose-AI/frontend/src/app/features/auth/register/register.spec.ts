import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';

import { Auth } from '../../../core/auth/services/auth';
import { Register } from './register';

describe('Register', () => {
  let component: Register;
  let fixture: ComponentFixture<Register>;
  let authService: jasmine.SpyObj<Auth>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    authService = jasmine.createSpyObj('Auth', ['register']);
    authService.register.and.returnValue(of({ message: 'ok' }));
    router = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [
        { provide: Auth, useValue: authService },
        { provide: Router, useValue: router },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Register);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should restore a previously saved registration draft', () => {
    localStorage.setItem(
      'safedose_registration_draft',
      JSON.stringify({
        currentStep: 3,
        step1: {
          fullName: 'Ahmed Ali',
          userName: 'ahmedali',
          phone: '+201234567890',
          email: 'ahmed@example.com',
        },
        step3: {
          termsAndConditions: true,
          permissions: ['medical_data', 'notifications'],
        },
        step4: {
          password: 'Password123!',
          confirmPassword: 'Password123!',
        },
      }),
    );

    component.ngOnInit();

    expect(component.currentStep).toBe(3);
    expect(component.step1Form.value).toEqual(
      jasmine.objectContaining({
        fullName: 'Ahmed Ali',
        userName: 'ahmedali',
        phone: '+201234567890',
        email: 'ahmed@example.com',
      }),
    );
    expect(component.selectedPermissions).toEqual(['medical_data', 'notifications']);
    expect(component.step4Form.value.password).toBe('Password123!');
  });

  it('should send the backend register payload expected by the API', () => {
    component.step1Form.setValue({
      fullName: 'Ahmed Ali',
      userName: 'ahmedali',
      phone: '+201234567890',
      email: 'ahmed@example.com',
    });
    component.step3Form.setValue({
      termsAndConditions: true,
      permissions: ['medical_data', 'notifications'],
    });
    component.step4Form.setValue({
      password: 'Password123!',
      confirmPassword: 'Password123!',
    });

    component.submit();

    const payload = authService.register.calls.most().args[0] as Record<string, unknown>;

    expect(payload).toEqual(
      jasmine.objectContaining({
        fullName: 'Ahmed Ali',
        userName: 'ahmedali',
        phoneNumber: '+201234567890',
        email: 'ahmed@example.com',
        password: 'Password123!',
        confirmPassword: 'Password123!',
        termsAndConditions: true,
      }),
    );
    expect(payload).not.toHaveProperty('age');
    expect(payload).not.toHaveProperty('conditions');
    expect(payload).not.toHaveProperty('emergencyContact');
    expect(payload).not.toHaveProperty('permissions');
  });
});
