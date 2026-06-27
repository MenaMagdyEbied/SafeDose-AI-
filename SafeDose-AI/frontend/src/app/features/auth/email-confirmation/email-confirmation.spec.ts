import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { Auth } from '../../../core/auth/services/auth';
import { EmailConfirmation } from './email-confirmation';

describe('EmailConfirmation', () => {
  let component: EmailConfirmation;
  let fixture: ComponentFixture<EmailConfirmation>;
  let authService: jasmine.SpyObj<Auth>;

  beforeEach(async () => {
    authService = jasmine.createSpyObj('Auth', ['confirmEmail', 'resendCode']);

    await TestBed.configureTestingModule({
      imports: [EmailConfirmation],
      providers: [{ provide: Auth, useValue: authService }],
    }).compileComponents();

    fixture = TestBed.createComponent(EmailConfirmation);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should resend the confirmation code for the current email', () => {
    component.email = 'test@example.com';
    authService.resendCode.and.returnValue(of({ message: 'تم الإرسال' } as never));

    component.resendCode();

    expect(authService.resendCode).toHaveBeenCalledWith('test@example.com');
  });
});
